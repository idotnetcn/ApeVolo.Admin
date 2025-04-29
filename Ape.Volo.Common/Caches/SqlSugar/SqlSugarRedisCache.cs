using System;
using System.Collections.Generic;
using Ape.Volo.Common.ConfigOptions;
using Ape.Volo.Common.Extensions;
using Newtonsoft.Json;
using SqlSugar;
using StackExchange.Redis;

namespace Ape.Volo.Common.Caches.SqlSugar;

/// <summary>
/// redis缓存
/// 实现SqlSugar.ICacheService
/// </summary>
public class SqlSugarRedisCache : ICacheService
{
    private static IDatabase Database => App.Cache.GetDatabase(App.GetOptions<RedisOptions>().Index + 1);


    public void Add<V>(string key, V value)
    {
        var valStr = value.ToJson();
        Database.StringSet(key, valStr);
    }

    public void Add<V>(string key, V value, int cacheDurationInSeconds)
    {
        var valStr = value.ToJson();
        var expireTime = new TimeSpan(0, 0, 0, cacheDurationInSeconds);
        Database.StringSet(key, valStr, expireTime);
    }

    public bool ContainsKey<V>(string key)
    {
        return Database.KeyExists(key);
    }

    public V Get<V>(string key)
    {
        var val = Database.StringGet(key);
        var redisValue = Database.StringGet(key);
        if (!redisValue.HasValue)
            return default;
        return JsonConvert.DeserializeObject<V>(val);
    }

    public IEnumerable<string> GetAllKey<V>()
    {
        var pattern = "SqlSugarDataCache.*";
        var redisResult = Database.ScriptEvaluate(LuaScript.Prepare(
            " local res = redis.call('KEYS', @keypattern) " +
            " return res "), new { keypattern = pattern });
        string[] preSult = (string[])redisResult;
        return preSult;
    }

    public V GetOrCreate<V>(string cacheKey, Func<V> create, int cacheDurationInSeconds = int.MaxValue)
    {
        if (ContainsKey<V>(cacheKey))
        {
            return Get<V>(cacheKey);
        }

        V val = create();
        Add(cacheKey, val, cacheDurationInSeconds);
        return val;
    }

    public void Remove<V>(string key)
    {
        Database.KeyDelete(key);
    }
}
