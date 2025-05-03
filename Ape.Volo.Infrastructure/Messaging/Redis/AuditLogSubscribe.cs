using Ape.Volo.Common.Attributes.Redis;
using Ape.Volo.Common.Extensions;
using Ape.Volo.Common.Helper;
using Ape.Volo.Core.Caches.Redis.MessageQueue;
using Ape.Volo.Entity.Logs;
using Ape.Volo.IBusiness.Monitor;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Ape.Volo.Infrastructure.Messaging.Redis;

public class AuditLogSubscribe : IRedisSubscribe
{
    #region Fields

    private readonly ILogger<AuditLogSubscribe> _logger;
    private readonly IAuditLogService _auditInfoService;

    #endregion

    #region Ctor

    public AuditLogSubscribe(IAuditLogService auditLogService, ILogger<AuditLogSubscribe> logger)
    {
        _auditInfoService = auditLogService;
        _logger = logger;
    }

    #endregion

    [SubscribeDelay(MqTopicNameKey.AuditLogQueue, true)]
    private async Task DoSub(List<RedisValue> redisValues)
    {
        try
        {
            if (redisValues.Any())
            {
                List<AuditLog> auditLogs = new List<AuditLog>();
                redisValues.ForEach(x => { auditLogs.Add(x.ToString().ToObject<AuditLog>()); });
                await _auditInfoService.CreateListAsync(auditLogs);
            }
        }
        catch (Exception e)
        {
            _logger.LogCritical(ExceptionHelper.GetExceptionAllMsg(e));
        }
    }
}
