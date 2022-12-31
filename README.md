# Xinjingdaily Bot

[![Codacy Badge](https://app.codacy.com/project/badge/Grade/67ca08867b7a4afda91db3b70bcd303c)](https://www.codacy.com/gh/chr233/XinjingdailyBot/dashboard?utm_source=github.com&utm_medium=referral&utm_content=chr233/XinjingdailyBot&utm_campaign=Badge_Grade)
![GitHub Workflow Status](https://img.shields.io/github/workflow/status/chr233/XinjingdailyBot/workflows/Publish.yml?branch=master&logo=github)
[![License](https://img.shields.io/github/license/chr233/XinjingdailyBot?logo=apache)](https://github.com/chr233/XinjingdailyBot/blob/master/license)
![GitHub last commit](https://img.shields.io/github/last-commit/chr233/XinjingdailyBot?logo=github)

[![GitHub Release](https://img.shields.io/github/v/release/chr233/XinjingdailyBot?logo=github)](https://github.com/chr233/XinjingdailyBot/releases)
[![GitHub Release](https://img.shields.io/github/v/release/chr233/XinjingdailyBot?include_prereleases&label=pre-release&logo=github)](https://github.com/chr233/XinjingdailyBot/releases)
[![GitHub Download](https://img.shields.io/github/downloads/chr233/XinjingdailyBot/total?logo=github)](https://img.shields.io/github/v/release/chr233/XinjingdailyBot)

![GitHub Repo stars](https://img.shields.io/github/stars/chr233/XinjingdailyBot?logo=github)

[![爱发电](https://img.shields.io/badge/爱发电-chr__-ea4aaa.svg?logo=github-sponsors)](https://afdian.net/@chr233)
[![Bilibili](https://img.shields.io/badge/bilibili-Chr__-00A2D8.svg?logo=bilibili)](https://space.bilibili.com/5805394)
[![Steam](https://img.shields.io/badge/steam-Chr__-1B2838.svg?logo=steam)](https://steamcommunity.com/id/Chr_)
[![Steam](https://img.shields.io/badge/steam-donate-1B2838.svg?logo=steam)](https://steamcommunity.com/tradeoffer/new/?partner=221260487&token=xgqMgL-i)

## 心惊报 [@xinjingdaily](https://t.me/xinjingdaily) 自主开发的投稿机器人

心惊报投稿机器人 [@xinjingdaily_bot](https://t.me/xinjingdaily_bot)

## 功能特性

- [x] 支持文字/图片/音频/视频投稿
- [x] 支持多图投稿
- [x] 支持匿名投稿
- [x] 支持直接发布投稿
- [x] 支持编辑标签
- [x] 支持过滤标签
- [x] 简单用户管理
  - [x] 设置用户组
  - [x] 封禁/解封
  - [x] 封禁记录查询
  - [x] 查看用户投稿统计
  - [x] 匿名回复
  - [x] 检索用户
- [ ] 审核超时自动拒绝
- [ ] 稿件检索
- [ ] 实装用户等级系统

## 安装与使用

从 [Releases](https://github.com/chr233/XinjingdailyBot/releases) 下载编译好的文件以后, 直接运行 XinjingDailyBot 即可

### 升级注意

数据库主键从 `long` 改为了 `int` , 如果从 `1.x` 升级至 `2.x` 后无法正常运行, 请重新创建数据库, 待程序生成数据库结构后使用工具手动导入旧的数据

自 `2.x` 后, 可选 `Sqlite` 作为数据库实现, 在配置文件的 `Database` 节设置 `UseMySQL` 为 `false` 即可

### 配置说明

配置文件为 `appsettings.json` 默认配置如下:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",

  // 调试模式
  "Debug": false,
  // 机器人设置
  "Bot": {
    "BotToken": "",
    "Proxy": null,
    "ThrowPendingUpdates": false,
    "AutoLeaveOtherGroup": false,
    "SuperAdmins": []
  },
  "Channel": {
    "ReviewGroup": "",
    "CommentGroup": "",
    "SubGroup": "",
    "AcceptChannel": "",
    "RejectChannel": ""
  },
  // 消息设置
  "Message": {
    "Start": "欢迎使用 心惊报 @xinjingdaily 专用投稿机器人",
    "Help": "发送图片/视频或者文字内容即可投稿"
  },
  // 数据库设置
  "Database": {
    "Generate": true,
    "UseMySQL": true,
    "LogSQL": false,
    "DbHost": "localhost",
    "DbPort": 3306,
    "DbName": "xjb_db",
    "DbUser": "root",
    "DbPassword": "123456"
  }
}
```

| 节       | 配置项              | 类型   | 默认值                                         | 说明                                    |
| -------- | ------------------- | ------ | ---------------------------------------------- | --------------------------------------- |
| 无       | Debug               | bool   | false                                          | 是否开启调试模式                        |
| -        | -                   | -      | -                                              |                                         |
| Bot      | BotToken            | string | ""                                             | 机器人 Token                            |
| Bot      | Proxy               | string | null                                           | 代理地址, 支持 http 和 sock5            |
| Bot      | ThrowPendingUpdates | bool   | false                                          | 启动时是否忽略机器人离线时产生的 Update |
| Bot      | AutoLeaveOtherGroup | bool   | false                                          | 是否自动离开无关群组                    |
| Bot      | SuperAdmins         | int[]  | []                                             | 超级管理员 数字 ID 列表                 |
| Bot      | BotToken            | string | ""                                             | 机器人 Token                            |
| Bot      | BotToken            | string | ""                                             | 机器人 Token                            |
| -        | -                   | -      | -                                              |                                         |
| Channel  | ReviewGroup         | string | ""                                             | 审核群组 ID                             |
| Channel  | CommentGroup        | string | ""                                             | 评论群组 ID                             |
| Channel  | SubGroup            | string | ""                                             | 闲聊群组 ID                             |
| Channel  | AcceptChannel       | string | ""                                             | 审核通过频道 ID                         |
| Channel  | RejectChannel       | string | ""                                             | 审核拒绝频道 ID                         |
| -        | -                   | -      | -                                              |                                         |
| Message  | Start               | string | "欢迎使用 心惊报 @xinjingdaily 专用投稿机器人" | 使用 /start 命令显示的欢迎语            |
| Message  | Help                | string | "发送图片/视频或者文字内容即可投稿"            | 使用 /help 命令显示语句                 |
| -        | -                   | -      | -                                              |                                         |
| Database | Generate            | bool   | true                                           | 是否自动生成数据库表                    |
| Database | UseMySQL            | bool   | true                                           | 是否使用 MySQL 作为数据库实现           |
| Database | LogSQL              | bool   | false                                          | 是否输出 SQL 日志                       |
| Database | DBHost              | string | "127.0.0.1"                                    | MySQL 主机                              |
| Database | DBPort              | int    | 3306                                           | MySQL 端口                              |
| Database | DBName              | string | "xjb_db"                                       | 数据库名                                |
| Database | DBUser              | string | "root"                                         | 数据库用户名                            |
| Database | DBPassword          | string | "123456"                                       | 数据库密码                              |

> 新安装或者数据库结构变动后一定要修改 `DBGenerate` 为 `true`, 会自动生成数据表

---

> `SuperAdmins` 机器人超级管理员的 UserID 列表, 覆盖数据库中的设定, 用户 UserID 可以使用命令 /myinfo 获取

---

> `AcceptChannel` 和 RejectChannel 必须为公开频道, 频道名需要加 `@`, 例如 `@xinjingdaily`

---

> `ReviewGroup`, `CommentGroup`, `SubGroup` 不一定需要是公开频道
> 如果是公开群组, 群组名需要加 `@`, 例如 `@xinjingdailychatroom`
> 如果是私有群组, 可以使用命令 /groupinfo 获取群组的信息, 然后设置为群组的 GroupID

### 权限说明

内置用户组权限如下

> 权限组的 `发布员` 目前有点问题，等待修复

| 组 ID | 组名           | 权限                                       | 说明                                                       |
| ----- | -------------- | ------------------------------------------ | ---------------------------------------------------------- |
| 0     | 封禁用户       | 无                                         | 无法直接设置用户到这个组, 被封禁的用户自动被视为此组的成员 |
| 1     | 普通用户       | 投稿,普通命令                              | 默认的用户组                                               |
| 10    | 审核员         | 投稿,审核投稿,普通命令                     | 具有审核权限的普通用户                                     |
| 11    | 发布员         | 投稿,直接发布,普通命令                     | 具有直接发布权限的普通用户                                 |
| 20    | 狗管理         | 投稿,审核投稿,直接发布,普通命令,管理员命令 | 具有所有投稿权限,可以使用普通管理员命令                    |
| 30    | 超级狗管理     | 所有权限(狗管理的权限 + 超级管理员命令)    | 具有所有投稿权限,可以使用所有命令                          |
| 50    | \*超级狗管理\* | 所有权限(狗管理的权限 + 超级管理员命令)    | 具有所有投稿权限,可以使用所有命令                          |

> 管理员仅能对用户组 ID 比自己小的对象(除了自己)进行操作, 例如狗管理(组 ID 为 20)无法操作超级狗管理(组 ID 为 30)

---

> 在 `config.json` 的 `SuperAdmins` 项中定义的管理员拥有最高的权限(组 ID 为 50)
> 虽然权限与超级狗管理(组 ID 为 30)相同, 但是因为具有更高的组 ID, 因此可以操作所有用户(除了自己)

### 命令说明

- 通用命令

> 任何用户组都能使用, 包括封禁用户

| 命令     | 参数 | 说明               |
| -------- | ---- | ------------------ |
| /start   | -    | 显示机器人欢迎语   |
| /help    | -    | 显示当前可用的命令 |
| /myban   | -    | 查询自己的封禁记录 |
| /version | -    | 显示机器人版本信息 |

- 普通命令

> 拥有`普通命令`权限的用户组可用

| 命令          | 参数 | 说明                                     |
| ------------- | ---- | ---------------------------------------- |
| /ping         | -    | 机器人存活测试                           |
| /anonymous    | -    | 仅限私聊, 设置投稿时是否默认使用匿名模式 |
| /notification | -    | 仅限私聊, 设置投稿被审核后是否接收通知   |
| /myinfo       | -    | 显示自己的投稿统计信息                   |
| /myright      | -    | 显示自己的权限信息                       |
| /admin        | -    | 仅限群聊, 艾特群组中的所有管理员         |

- 审核命令

> 拥有`审核`权限的用户组可用

| 命令  | 参数 | 说明                 |
| ----- | ---- | -------------------- |
| /no   | 理由 | 用自定义理由拒绝稿件 |
| /edit | 描述 | 修改稿件的描述信息   |

- 管理员命令

> 拥有`管理员命令`权限的用户组可用

| 命令       | 参数                     | 说明                         |
| ---------- | ------------------------ | ---------------------------- |
| /groupinfo | -                        | 仅限群聊使用, 查看群组信息   |
| /userinfo  | \[UserName/UserID\]      | 获取指定用户的信息           |
| /ban       | \[UserName/UserID\] 理由 | 封禁指定用户                 |
| /unban     | \[UserName/UserID\] 理由 | 解封指定用户                 |
| /queryban  | \[UserName/UserID\]      | 显示指定用户的封禁记录       |
| /echo      | \[UserName/UserID\] 消息 | 通过机器人向指定用户发送消息 |
| /queryuser | 关键词 \[页码\]          | 通过关键词查找用户           |
| /sysreport | -                        | 查看机器人统计信息           |
| /invite    | -                        | 生成审核群的邀请链接         |
| /userrank  | -                        | 显示用户投稿数据排行榜       |

- 超级管理员命令

> 拥有`超级管理员命令`权限的用户组可用

| 命令          | 参数                | 说明                 |
| ------------- | ------------------- | -------------------- |
| /restart      | -                   | 重启机器人           |
| /setusergroup | \[UserName/UserID\] | 修改指定用户的用户组 |
