---
AIGC:
    Label: "1"
    ContentProducer: 001191440300708461136T1XGW3
    ProduceID: 976c43aeada9220b2c6a8829824087ba_c36b41c7c30111f1a05452540064ee0f
    ReservedCode1: xI/Jf/lvAowAC+8hof65cz86OYYYm1VfFGhFFe6pHHhMdw1IYBrTGA+X6yiLP4KpYWTo5OwPoh26iTuMiinTBgVrmdT9OwS7GjR7K7VXnImzqZ07K9DOSK4VmeVhOCNAG85pCvbj9Y2BxWVpQLcZcaXX/5JdFKBDY9aJ8tZYlLKMPXJRCTHO432uUaw=
    ContentPropagator: 001191440300708461136T1XGW3
    PropagateID: 976c43aeada9220b2c6a8829824087ba_c36b41c7c30111f1a05452540064ee0f
    ReservedCode2: xI/Jf/lvAowAC+8hof65cz86OYYYm1VfFGhFFe6pHHhMdw1IYBrTGA+X6yiLP4KpYWTo5OwPoh26iTuMiinTBgVrmdT9OwS7GjR7K7VXnImzqZ07K9DOSK4VmeVhOCNAG85pCvbj9Y2BxWVpQLcZcaXX/5JdFKBDY9aJ8tZYlLKMPXJRCTHO432uUaw=
---

# XinjingDaily Bot v3.0 重构设计文档

- 日期：2026-10-08
- 状态：已确认（用户评审通过）
- 范围：XinjingdailyBotNew（v3.0）全量重写，旧版 XinjingdailyBot（v2.x）作为需求参考

## 1. 背景与目标

心惊报 Telegram 投稿机器人从旧版 v2.x（H:\repos\XinjingdailyBot）重构为 v3.0（H:\repos\XinjingdailyBotNew，开发中）。新版已具备技术栈与项目骨架（.NET 10 / SqlSugar / Redis / Quartz / SignalR / Roslyn 源生成器），本设计在此基础上全量重写业务架构。

六大目标需求：

1. 用户系统：经验值、积分、个人信息、道具系统、用户身份系统
2. 交互式投稿：上下文连续对话、满 9 条媒体自动截止、文本自动拼接
3. 投稿流程使用 Context 系统
4. 系统逻辑事件驱动，设计充足事件，预留 Plugin 接口
5. 网页访问接口：Telegram 登录、用户界面（随机投稿/个人信息）、管理员设置、广告主广告管理、图片文字上传
6. 命令系统：源生成器自动注册，避免反射

补充需求（用户确认）：

- 经验按发言与投稿获取，设置中可配系数与每日上限
- 投稿双模式：一次性（旧版一致）与渐进式（多条消息合并）
- 多频道支持：命令选择投稿频道，审核多选发布频道，管理员动态设置频道
- 用户体系独立（Discuz UCenter 模式）：多 Bot 共享同一用户体系
- 多 Bot 预留接口：BotId 扩展点 + 主从 SignalR RPC（主 Bot 持库，从 Bot RPC 读写）
- 反馈采集与榜单：定时扫描频道 reactions 与评论，收集投稿反馈，定期生成榜单

## 2. 方案选型

**方案 A：全量重写（已选）**

- 保留新版仓库已验证的技术栈与项目骨架（.NET 10 / SqlSugar / Redis / Quartz / SignalR / 源生成器项目）
- 业务架构与代码全部按目标架构重写，旧版只当需求参考
- 不兼容旧代码路径，回归测试在 M6 统一验证

关键约束：

- Release 启用 TrimMode Full + PublishSingleFile + ReadyToRun，运行时反射受限
- 命令/插件/事件处理器/实体注册必须走源生成器（零反射），AOT 友好
- 全局中性语言 zh-cn，LangVersion latest，Nullable enable

## 3. 总体架构

### 3.1 分层结构

| 项目 | 职责 |
|---|---|
| `XinjingDaily.Bot.App` | WebApplication 宿主，DI 组装、中间件、启动编排 |
| `XinjingDaily.Bot.Generator` | Roslyn 源生成器：命令/事件处理器/插件/实体注册表（M6 完整实现，此前空骨架占位） |
| `XinjingDaily.Bot.Infrastructure` | 配置、枚举、本地化、Utils、事件总线内核、Context 内核、Plugin 契约、注册机制双模式 |
| `XinjingDaily.Bot.Entry` | SqlSugar 实体（用户/投稿/审核/广告/道具/积分/频道等） |
| `XinjingDaily.Bot.IRepository` / `Repository` | 仓储接口与实现（含 Redis 缓存、RPC 代理实现） |
| `XinjingDaily.Bot.Interface` / `Service` | 业务服务：用户、投稿、审核、广告、网页认证 |
| `XinjingDaily.Bot.Command` | 命令处理器 + 事件处理器（特性标注，注册表收集） |
| `XinjingDaily.Bot.Controllers` | Web API：网页端接口 + Telegram 登录 |
| `XinjingDaily.Bot.SignalR` | SignalR Hub：BotRpcHub（主从 RPC）、PluginHub（网页/插件实时） |
| `XinjingDaily.Bot.Tasks` | Quartz 定时任务：过期稿件、定时发布、广告计划、审核超时 |
| `XinjingDaily.Bot.Data` | 共享 DTO / 请求响应模型 |

### 3.2 消息处理主链路

```
Telegram Update
  → TelegramBotService（唯一入口，解析消息）
  → BotContextFactory（构建单次消息上下文）
  → 命令匹配（注册表，零反射）→ 权限校验 → 命令处理器 → 业务 Service
  → 状态机分发（投稿流程）→ SessionContext 读写
  → 事件总线 EventBus（异步分发，异常隔离）→ 事件处理器
  → 回复
```

### 3.3 三条铁律

1. 零反射：命令、插件、事件处理器、实体全部由源生成器编译期生成注册代码；DEBUG 期间用反射实现兜底（见 4.1）
2. 事件驱动：业务副作用一律走事件总线，Service 之间不直接互相调用
3. Context 驱动交互：一切多轮对话状态落在 Context 系统，进程重启可从 Redis/DB 恢复

## 4. 核心机制

### 4.1 注册机制双模式

```
统一特性标注层
  ├─ [Command("name", 权限)]        → 命令
  ├─ IEventHandler<TEvent> 泛型接口 → 事件处理器
  ├─ [Plugin("id")] IPlugin         → 插件
  └─ [SugarTableEntry]              → 数据库实体

注册表构建（条件编译）
  #if DEBUG    → ReflectionRegistryLoader（反射扫描，快速迭代）
  #if RELEASE  → GeneratedRegistryLoader（源生成器静态注册表，零反射）

统一出口：IRegistryLoader.Load() → CommandRegistry / EventHandlerRegistry / PluginRegistry / TableEntryRegistry
```

- M6 前：源生成器项目放空骨架（生成空注册表占位），Release 构建暂时走反射版兜底并打警告
- M6：填充三个源生成器，Release 构建验证零反射 + AOT 裁剪通过

### 4.2 事件总线

- `IEvent`：标记接口，数据用 readonly record struct 承载
- `IEventHandler<TEvent>`：`Task Handle(TEvent e, CancellationToken ct)`
- `EventBus`：`PublishAsync(IEvent)` 异步分发；同事件处理器顺序执行保证确定性；异常隔离（单处理器失败不影响总线，记日志继续）
- 插件通过注册 `IEventHandler<TEvent>` 实现无侵入监听

事件清单（第一版）：

| 域 | 事件 |
|---|---|
| 用户 | UserRegistered、UserExpChanged、UserLevelUp、UserPointsChanged、ItemGranted、ItemUsed、ItemExpired、UserIdentityChanged、UserBanned |
| 投稿 | PostFlowStarted、MediaCollected、TextCollected、PostFlowCompleted、PostSubmitted、PostApproved、PostRejected、PostPublished、PostExpired |
| 广告 | AdCreated、AdChanged、AdActivated、AdExpired |
| 反馈 | FeedbackCollected、PostStatsUpdated、RankingGenerated |
| 系统 | BotStarted、BotStopping、ConfigChanged、ChannelChanged、PluginLoaded、PluginUnloaded |

### 4.3 Context 三级键空间

| 类型 | 键 | 内容 | 用途 |
|---|---|---|---|
| `UserContext` | (UserId) | 身份、经验、积分、道具、个人信息、全局设置 | 用户维度，跨会话共享 |
| `ChatContext` | (ChatType, ChatId) | 群投稿开关、群设置、审核配置、广告状态 | 群组维度，同群共享 |
| `SessionContext` | (UserId, ChatType, ChatId) | 投稿状态机、草稿、素材列表、多轮对话状态 | 用户×会话维度，完全隔离 |

会话隔离规则：

- 同一用户在群 A / 群 B / 私聊分别持有独立 SessionContext，互不继承
- 同群多人同时投稿不冲突（键含 UserId）
- ChatType 用 Telegram 原生枚举：Private / Group / Supergroup / Channel
- 投稿状态机挂 SessionContext：Idle → Collecting → Complete → Canceled

存储：

```
Redis Key 规划：
  user:ctx:{userId}                        → UserContext
  chat:ctx:{chatType}:{chatId}             → ChatContext
  session:ctx:{userId}:{chatType}:{chatId} → SessionContext（含投稿草稿 JSON）
```

`IContextService` 统一提供 GetOrCreate / Update / Clear，Redis 活跃态 + DB 兜底。

## 5. 用户系统（UCenter 模式）

### 5.1 共享与隔离边界

- 共享（全局用户体系，键 = Telegram UserId）：Users（资料/经验/等级/积分）、ExpLogs、PointsLogs、Items、UserItems（道具背包）
- 隔离（Bot 实例级，键 = BotId）：身份权限 UserPermissions(UserId, BotId, Identity)、投稿、审核、频道、广告、群组设置、SessionContext

### 5.2 数据模型

| 表 | 关键字段 | 说明 |
|---|---|---|
| Users | UserId、UserName、NickName、Avatar、Exp、Level、Points、IsBanned、CreatedAt、LastActiveAt | 用户主表 |
| UserPermissions | UserId、BotId、Identity | 实例级身份：NormalUser / Reviewer / Admin / SuperAdmin / Advertiser |
| ExpLogs | UserId、Delta、Reason、CreatedAt | 经验流水 |
| PointsLogs | UserId、Delta、Reason、Balance、CreatedAt | 积分流水 |
| Items | ItemId、Name、Description、EffectType、Price、Icon、Enabled | 道具定义 |
| UserItems | UserId、ItemId、Count、ExpireAt | 用户背包 |
| UserItemLogs | UserId、ItemId、Delta、Reason、CreatedAt | 道具流水 |

### 5.3 核心规则

| 系统 | 规则 |
|---|---|
| 经验 | 来源：群内发言 + 投稿（投稿行为 + 稿件被采用）；设置项 ExpSettings = { MessageExp, PostExp, AcceptedExp, DailyLimit }，管理员可动态改；每日上限防刷；曲线 LevelUpExp(level) = base × level² |
| 等级 | 升级发 UserLevelUp 事件，可配置升级奖励 |
| 积分 | 获取：投稿采用、签到、活动；消耗：商店购买道具；全变动写 PointsLogs |
| 道具 | 展示类（头像框/称号）、功能类（改名卡、投稿加急、额外额度）；商店购买、升级奖励、活动发放；/use 使用走事件 |
| 身份 | 全局 SuperAdmin 跨 Bot 共享；实例身份按 BotId 隔离；身份变更发 UserIdentityChanged |

### 5.4 命令集

/profile、/points、/shop、/buy、/bag、/use、/rank、/setidentity（Admin+）

## 6. 投稿系统

### 6.1 双模式

| 模式 | 触发 | 行为 |
|---|---|---|
| 一次性（旧版一致） | 群内投稿开启时直接发送内容，或 /quickpost | 单条消息/单个相册直接作为完整稿件提交 |
| 渐进式（新版交互） | /post 启动 | 多条消息累积（媒体最多 9 条、文本自动拼接），/submit 或满 9 条合并提交 |

- 群内默认模式可配置
- 两种模式产出同一 Posts 结构，后续审核链路一致

### 6.2 渐进式状态机（挂在 SessionContext）

```
Idle ──/post──▶ Collecting ──满9条媒体──▶ Complete ──▶ Submitted
                  │   ▲                        │
                  │   └── 继续收素材 ───────────┘（/submit 手动截止）
                  ▼
               Canceled（/cancel 或超时）
```

草稿模型：

```
Draft { ChatType, ChatId, UserId, State,
        MediaList: List<MediaItem>,   // FileId + 类型（Photo/Video/Document/Animation/Audio）
        TextBuilder,                  // 文本自动拼接
        StartedAt, UpdatedAt }
```

交互规则：

- 连续发媒体：MediaCollected 事件累计计数，满 9 条自动截止 → Complete → "已满9条，自动提交"
- 发送文本：TextCollected 事件按顺序自动拼接（换行分隔），支持混排
- 相册 MediaGroup：一组按一合并计数
- /submit、/done 手动截止；/cancel 取消并清 SessionContext；/status 查看状态
- 超时（默认 30 分钟，可配置）提醒后自动取消

### 6.3 多频道

| 项 | 设计 |
|---|---|
| Channels 表 | ChannelId(Telegram)、Name、Tag、Enabled、Remark、PostCount |
| 投稿选频道 | /post #频道名 或渐进式流程中 /channel 切换；默认频道兜底 |
| 审核多选发布 | /approve 时多选频道，默认勾选投稿目标频道，结果存 PostChannels 关联表 |
| 动态管理 | /channel list/add/edit/remove（Admin+）；网页端同步支持；变更发 ChannelChanged 事件，实时生效 |

## 7. 核心业务链路

### 7.1 审核

```
PostSubmitted → 审核队列 → 通知审核人（审核群 inline button：通过/拒绝/跳过）
  → PostApproved（多选频道）→ PostPublished（逐频道发布）→ 通知投稿人 + 发放采用经验
  → PostRejected（附原因）→ 通知投稿人，可修改后重新投稿
```

- 审核交互：inline button 与命令双通道
- 定时发布：稿件可设发布时间，Quartz 到点发布
- 审核超时提醒：滞留稿件可配置超时提醒审核人

### 7.2 广告（广告主身份）

- AdPlans（广告主、内容、投放频道、生效/到期时间、状态）+ AdContents（文字/图片）
- 广告计划按时间窗在指定频道穿插发布，Quartz 检查生效/到期
- 广告主网页端设置内容（图片/文字上传），Admin 可查看/停用

### 7.3 新增表

Posts、PostChannels、ReviewLogs、Channels、AdPlans、AdContents、PostStats、Rankings、FeedbackSettings（全部带 BotId 列）

### 7.4 反馈采集与榜单

目标：定时扫描频道已发布稿件的 reactions（reaction 计数）与评论（讨论组回复），收集投稿反馈数据，定期生成榜单。

#### 7.4.1 数据模型

| 表 | 关键字段 | 说明 |
|---|---|---|
| PostStats | PostId、ChannelMessageId、Views、ReactionCount、CommentCount、UpdatedAt | 稿件单条反馈快照（增量更新） |
| Rankings | RankingId、RankingType（Daily/Weekly/Monthly）、Title、StartAt、EndAt、SnapshotJson、CreatedAt | 榜单快照，SnapshotJson 存 TopN 明细（PostId、Title、Score、Rank） |
| FeedbackSettings | BotId、ScanIntervalMinutes、ScanWindowDays、ScoreWeights、TopN、Enabled | 扫描间隔、回溯窗口、评分权重（Views/Reactions/Comments）、榜单位置数 |

#### 7.4.2 采集链路

- 定时任务 FeedbackScanTask（Quartz）：按 FeedbackSettings 间隔（默认每 60 分钟）扫描回溯窗口（默认最近 7 天）内已发布的稿件消息
- 采集源：Telegram API 频道消息的 views / reaction 计数；评论数取消息关联讨论组（DiscussionGroup）线程回复量
- 增量采集：记录已扫描的 (ChannelId, MessageId, LastScannedAt) 锚点，避免重复全量扫描；仅更新发生变化的稿件
- 每次扫描完成发布 FeedbackCollected 事件；统计值有变化时发布 PostStatsUpdated

#### 7.4.3 榜单生成

- 定时任务 RankingGenerateTask（Quartz）：每日 / 每周 / 每月按窗口生成榜单
- 评分公式：Score = Views × wViews + ReactionCount × wReactions + CommentCount × wComments（权重来自 FeedbackSettings，默认 1:2:3，可配置）
- 生成完成发布 RankingGenerated 事件，可挂钩：频道自动发布榜单消息、上榜投稿人发放经验/积分奖励（配合用户系统）
- 查询命令：/rank（查看当前榜单）、/rank top 10、/rank daily|weekly|monthly（切换周期）

#### 7.4.4 管理配置

- /ranksettings（Admin+）：修改扫描间隔、回溯窗口、评分权重、TopN、启停
- 网页端管理员页同步展示与修改 FeedbackSettings
- 变更发 ConfigChanged 事件热生效

## 8. 网页端

### 8.1 Telegram 登录

- 发码登录（推荐）：/login → 一次性登录码（5 分钟有效）→ 网页输入码换 JWT Session；无需公网回调域名
- Widget 登录（可选）：有公网域名时启用，并存

### 8.2 页面与 API

| 页面 | 功能 | API |
|---|---|---|
| 登录页 | 输入登录码换取会话 | POST /api/auth/login |
| 用户主页 | 个人信息（等级/经验/积分/背包）、随机投稿浏览、榜单查看 | GET /api/me、GET /api/posts/random、GET /api/rankings |
| 管理员页 | 修改机器人设置（经验系数/每日上限/投稿模式/频道 CRUD/反馈扫描与榜单配置） | GET·PUT /api/settings、/api/channels、/api/feedback-settings |
| 广告主页 | 广告内容管理、图片/文字上传 | GET·POST /api/ads、POST /api/ads/{id}/media |

### 8.3 技术形态

- 前端：wwwroot 静态页（HTML/JS）+ Web API，兼容单文件发布
- 认证：JWT（发码交换），身份按 UserPermissions（BotId 实例级）判定
- 实时：PluginHub 推稿件状态变化（可选增强）
- 设置动态生效：管理员改设置 → 存 DB → 发 ConfigChanged 事件 → 服务热生效

## 9. 多 Bot 与主从 RPC

### 9.1 必要性结论

当前单 Bot 场景不完整实现多 Bot 路由（YAGNI），只保留扩展点：Posts/Drafts/事件携带来源 BotId（默认常量 1）；ITelegramBotService 已接口化，将来注册多实例即可。

### 9.2 主从拓扑（SignalR RPC）

```
主 Bot（持 DB + Redis）
  ▲ │
  │ │ SignalR（BotRpcHub，带鉴权）
  │ ▼
从 Bot A / 从 Bot B（无数据库，全部数据访问走 RPC）
```

- 数据访问双模式：IRepository 体系（现有接口不变）→ 本地实现（主 Bot 直连 SqlSugar/Redis）/ RPC 代理实现（从 Bot SignalR InvokeAsync 转发）
- 配置开关：DatabaseMode = Local | Rpc，一行切换，业务代码零改动
- BotRpcHub：强类型仓储级方法（GetUserAsync / UpdateUserPointsAsync / CreatePostAsync 等），编译期安全、零反射；响应泛型 RpcResult<T>；握手携带 BotToken 鉴权；写操作带 OperationId 幂等键；断线自动重连
- 从 Bot 连接后仅能访问自身 BotId 数据范围

## 10. 时间节点（19 周 / 约 4.5 个月，单人全职估算）

| 里程碑 | 周期 | 交付物 | 验收标准 |
|---|---|---|---|
| M1 架构骨架 | 第 1-2 周 | 事件总线、Context 三级键空间、注册双模式框架（反射版可用 + 源生成器空占位）、Plugin 契约、IRepository 双实现抽象 | 跑通 demo 命令 + demo 事件 |
| M2 用户系统 | 第 3-5 周 | 用户/身份/经验/积分/道具全链路 + 命令 + Web API（UCenter 模式） | 用户可查积分、升级、用道具 |
| M3 投稿重构 | 第 6-8 周 | 投稿双模式状态机、9 条媒体截止、文本拼接、多频道选择、Context 持久化 | 群内完整投一篇稿（图文混排） |
| M4 核心链路 | 第 9-13 周 | 审核（多选频道发布）/定时发布/标签/广告计划/反馈采集与榜单生成，全部事件驱动 | 投稿→审核→多频道发布→频道全通；榜单按时生成并可查询 |
| M5 网页端 | 第 14-16 周 | Telegram 发码登录、用户界面、管理员设置（含榜单配置）、广告主后台、图片文字上传 | 三类身份网页端完整可用 |
| M6 插件与发布 | 第 17-19 周 | 源生成器完整实现（命令/事件/插件/实体注册表）、BotRpcHub + RPC 仓储代理、官方插件示例、单元/集成测试、文档、Docker/单文件发布 | Release 零反射 + AOT 通过；主从联调通过 |

## 11. 测试与发布

- 单元测试：命令注册、状态机、积分计算、事件总线、经验上限、榜单评分计算
- 集成测试：投稿流程端到端（mock Telegram）、审核发布链路、反馈扫描与榜单生成（mock 频道消息统计）
- Web API 测试：认证、权限、上传、榜单查询
- 发布验证：Release 构建验证 TrimMode Full 下无反射告警、AOT 裁剪通过、单文件发布可运行
- 部署：Docker（docker-compose 保留）与单文件两种形态

## 12. 决策记录

| 决策 | 结论 |
|---|---|
| 重写路线 | 方案 A 全量重写（保留技术栈与项目骨架） |
| 注册机制 | DEBUG 反射 / RELEASE 源生成器双模式，源生成器 M6 实现 |
| Context 隔离 | 三级键空间，投稿挂 SessionContext |
| 用户体系 | UCenter 模式：全局共享用户/积分/道具，实例级权限 |
| 多 Bot | 仅留 BotId 扩展点 + 主从 SignalR RPC，不做完整路由 |
| 多频道 | Channels 表 + 投稿选频道 + 审核多选发布 + 动态管理 |
| 反馈与榜单 | Quartz 增量扫描 reactions/评论 → PostStats → 周期榜单（Rankings 快照），事件驱动、网页端可查可配 |
| 网页登录 | 发码登录为主，Widget 可选 |
*（内容由AI生成，仅供参考）*
