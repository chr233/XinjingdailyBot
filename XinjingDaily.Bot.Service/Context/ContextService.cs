using Microsoft.Extensions.Logging;
using System.Text.Json;
using XinjingDaily.Bot.Entry.Entries.Context;
using XinjingDaily.Bot.Interface.Context;
using XinjingDaily.Bot.IRepository.Context;

namespace XinjingDaily.Bot.Service.Bot.Context;

[RegisterSingleton(Registration = RegistrationStrategy.ImplementedInterfaces)]
public class ContextService : IContextService
{
    private readonly ILogger<ContextService> _logger;
    private readonly IUserContextRepository _userCtxRepo;
    private readonly IChatContextRepository _chatCtxRepo;
    private readonly IContextRedisRepository _redis;

    public ContextService(
        ILogger<ContextService> logger,
        IUserContextRepository userCtxRepo,
        IChatContextRepository chatCtxRepo,
        IContextRedisRepository redis)
    {
        _logger = logger;
        _userCtxRepo = userCtxRepo;
        _chatCtxRepo = chatCtxRepo;
        _redis = redis;
    }

    public PrivateContext CreatePrivateContext(int userId, long chatId)
    {
        var store = new LazyContextStore(() => LoadUserDtoAsync(userId, chatId));
        return new PrivateContext(store);
    }

    public GroupContext CreateGroupContext(int userId, long chatId, string command)
    {
        var userStore = new LazyContextStore(() => LoadUserDtoAsync(userId, chatId));
        var chatStore = new LazyContextStore(() => LoadChatDtoAsync(command, chatId));
        return new GroupContext(userStore, chatStore);
    }

    private async Task<ContextRedisDto> LoadUserDtoAsync(int userId, long chatId)
    {
        var dto = await _redis.GetUserContextAsync(userId, chatId).ConfigureAwait(false);
        if (dto is not null) return dto;

        var entry = await _userCtxRepo.QueryAsync(userId, chatId).ConfigureAwait(false);
        if (entry is not null)
        {
            return new ContextRedisDto {
                DbId = entry.Id,
                UserId = entry.UserId,
                ChatId = entry.ChatId,
                Mode = entry.Mode ?? string.Empty,
                Data = TryDeserializeData(entry.DataJson)
            };
        }

        return new ContextRedisDto { UserId = userId, ChatId = chatId };
    }

    private async Task<ContextRedisDto> LoadChatDtoAsync(string command, long chatId)
    {
        var dto = await _redis.GetChatContextAsync(command, chatId).ConfigureAwait(false);
        if (dto is not null) return dto;

        var entry = await _chatCtxRepo.QueryAsync(command, chatId).ConfigureAwait(false);
        if (entry is not null)
        {
            return new ContextRedisDto {
                DbId = entry.Id,
                Command = entry.Command,
                ChatId = entry.ChatId,
                Mode = entry.Mode ?? string.Empty,
                Data = TryDeserializeData(entry.DataJson)
            };
        }

        return new ContextRedisDto { Command = command, ChatId = chatId };
    }

    public async Task SavePrivateContextAsync(PrivateContext ctx)
    {
        if (!ctx.IsDirty) return;
        var dto = ctx.Store.ExportDto();
        if (dto is null) return;
        await SaveUserStoreAsync(dto).ConfigureAwait(false);
        ctx.Store.MarkClean();
    }

    public async Task SaveGroupContextAsync(GroupContext ctx)
    {
        var tasks = new List<Task>(2);
        if (ctx.IsUserDirty)
        {
            var dto = ctx.UserStore.ExportDto();
            if (dto is not null) tasks.Add(SaveUserStoreAsync(dto).ContinueWith(_ => ctx.UserStore.MarkClean()));
        }
        if (ctx.IsChatDirty)
        {
            var dto = ctx.ChatStore.ExportDto();
            if (dto is not null) tasks.Add(SaveChatStoreAsync(dto).ContinueWith(_ => ctx.ChatStore.MarkClean()));
        }
        if (tasks.Count > 0) await Task.WhenAll(tasks).ConfigureAwait(false);
    }

    private async Task SaveUserStoreAsync(ContextRedisDto dto)
    {
        var entry = new UserContextEntry {
            Id = dto.DbId,
            UserId = dto.UserId,
            ChatId = dto.ChatId,
            Mode = string.IsNullOrEmpty(dto.Mode) ? null : dto.Mode,
            DataJson = JsonSerializer.Serialize(dto.Data)
        };
        await _userCtxRepo.UpsertAsync(entry).ConfigureAwait(false);
        dto.DbId = entry.Id;

        var ok = await _redis.SetUserContextAsync(dto.UserId, dto.ChatId, dto).ConfigureAwait(false);
        if (!ok) _logger.LogWarning("[Context] Redis 写回 user_context 失败 u={U} c={C}", dto.UserId, dto.ChatId);
    }

    private async Task SaveChatStoreAsync(ContextRedisDto dto)
    {
        var entry = new ChatContextEntry {
            Id = dto.DbId,
            Command = dto.Command,
            ChatId = dto.ChatId,
            Mode = string.IsNullOrEmpty(dto.Mode) ? null : dto.Mode,
            DataJson = JsonSerializer.Serialize(dto.Data)
        };
        await _chatCtxRepo.UpsertAsync(entry).ConfigureAwait(false);
        dto.DbId = entry.Id;

        var ok = await _redis.SetChatContextAsync(dto.Command, dto.ChatId, dto).ConfigureAwait(false);
        if (!ok) _logger.LogWarning("[Context] Redis 写回 chat_context 失败 cmd={Cmd} c={C}", dto.Command, dto.ChatId);
    }

    private static Dictionary<string, string> TryDeserializeData(string json)
    {
        try { return JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? []; }
        catch { return []; }
    }
}
