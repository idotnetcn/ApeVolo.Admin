using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ape.Volo.Common.Extensions;
using Ape.Volo.Common.Global;
using Ape.Volo.Common.Model;
using Ape.Volo.Common.WebApp;
using Ape.Volo.Core.Caches;
using Ape.Volo.Entity.Core.System;
using Ape.Volo.IBusiness.Monitor;
using Ape.Volo.IBusiness.System;
using Ape.Volo.SharedModel.Queries.Common;
using Ape.Volo.ViewModel.Report.Monitor;

namespace Ape.Volo.Business.Monitor;

/// <summary>
/// 在线用户服务
/// </summary>
public class OnlineUserService : IOnlineUserService
{
    private readonly ICache _cache;
    private readonly ITokenBlacklistService _tokenBlacklistService;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="cache"></param>
    /// <param name="tokenBlacklistService"></param>
    public OnlineUserService(ICache cache, ITokenBlacklistService tokenBlacklistService)
    {
        _cache = cache;
        _tokenBlacklistService = tokenBlacklistService;
    }

    /// <summary>
    /// 查询
    /// </summary>
    /// <param name="pagination"></param>
    /// <returns></returns>
    public async Task<List<LoginUserInfo>> QueryAsync(Pagination pagination)
    {
        List<LoginUserInfo> loginUserInfos = new List<LoginUserInfo>();
        var arrayList = await _cache.ScriptEvaluateKeys(GlobalConstants.CachePrefix.OnlineKey);
        if (arrayList.Length > 0)
        {
            foreach (var item in arrayList)
            {
                var loginUserInfo =
                    await _cache.GetAsync<LoginUserInfo>(item);
                if (loginUserInfo.IsNull()) continue;
                loginUserInfo.AccessToken = loginUserInfo.AccessToken.ToMd5String16();
                loginUserInfos.Add(loginUserInfo);
            }
        }

        List<LoginUserInfo> newOnlineUsers = new List<LoginUserInfo>();
        if (loginUserInfos.Count > 0)
        {
            newOnlineUsers = loginUserInfos.Skip((pagination.PageIndex - 1) * pagination.PageSize)
                .Take(pagination.PageSize)
                .ToList();
        }

        return newOnlineUsers;
    }

    /// <summary>
    /// 强退
    /// </summary>
    /// <param name="ids"></param>
    public async Task DropOutAsync(HashSet<string> ids)
    {
        var list = new List<TokenBlacklist>();
        list.AddRange(ids.Select(x => new TokenBlacklist { AccessToken = x }));
        if (await _tokenBlacklistService.AddAsync(list))
        {
            foreach (var item in ids)
            {
                await _cache.RemoveAsync(GlobalConstants.CachePrefix.OnlineKey + item);
            }
        }
    }

    /// <summary>
    /// 下载
    /// </summary>
    /// <returns></returns>
    public async Task<List<ExportBase>> DownloadAsync()
    {
        List<ExportBase> onlineUserExports = new List<ExportBase>();
        var arrayList = await _cache.ScriptEvaluateKeys(GlobalConstants.CachePrefix.OnlineKey);
        if (arrayList.Length > 0)
        {
            foreach (var item in arrayList)
            {
                LoginUserInfo loginUserInfo =
                    await _cache.GetAsync<LoginUserInfo>(item);
                if (loginUserInfo != null)
                {
                    onlineUserExports.Add(loginUserInfo.ChangeType<OnlineUserExport>());
                }
            }
        }

        return onlineUserExports;
    }
}
