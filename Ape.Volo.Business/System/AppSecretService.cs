using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ape.Volo.Common.Extensions;
using Ape.Volo.Common.Global;
using Ape.Volo.Common.IdGenerator;
using Ape.Volo.Common.Model;
using Ape.Volo.Core;
using Ape.Volo.Core.ConfigOptions;
using Ape.Volo.Core.Utils;
using Ape.Volo.Entity.Core.System;
using Ape.Volo.IBusiness.System;
using Ape.Volo.SharedModel.Dto.Core.System;
using Ape.Volo.SharedModel.Queries.Common;
using Ape.Volo.SharedModel.Queries.System;
using Ape.Volo.ViewModel.Core.System;
using Ape.Volo.ViewModel.Report.System;

namespace Ape.Volo.Business.System;

/// <summary>
/// App应用秘钥
/// </summary>
public class AppSecretService : BaseServices<AppSecret>, IAppSecretService
{
    #region 基础方法

    /// <summary>
    /// 创建
    /// </summary>
    /// <param name="createUpdateAppSecretDto"></param>
    /// <returns></returns>
    public async Task<OperateResult> CreateAsync(CreateUpdateAppSecretDto createUpdateAppSecretDto)
    {
        if (await TableWhere(r => r.AppName == createUpdateAppSecretDto.AppName).AnyAsync())
        {
            return OperateResult.Error(ValidationError.IsExist(createUpdateAppSecretDto,
                nameof(createUpdateAppSecretDto.AppName)));
        }

        var id = IdHelper.NextId().ToString();
        createUpdateAppSecretDto.AppId = DateTime.Now.ToString("yyyyMMdd") + id[..8];
        createUpdateAppSecretDto.AppSecretKey =
            (createUpdateAppSecretDto.AppId + id).ToHmacsha256String(App.GetOptions<SystemOptions>().HmacSecret);
        var appSecret = App.Mapper.MapTo<AppSecret>(createUpdateAppSecretDto);
        var result = await AddAsync(appSecret);
        return OperateResult.Result(result);
    }

    /// <summary>
    /// 更新
    /// </summary>
    /// <param name="createUpdateAppSecretDto"></param>
    /// <returns></returns>
    public async Task<OperateResult> UpdateAsync(CreateUpdateAppSecretDto createUpdateAppSecretDto)
    {
        //取出待更新数据
        var oldAppSecret = await TableWhere(x => x.Id == createUpdateAppSecretDto.Id).FirstAsync();
        if (oldAppSecret.IsNull())
        {
            return OperateResult.Error(ValidationError.NotExist(createUpdateAppSecretDto,
                LanguageKeyConstants.AppSecret,
                nameof(createUpdateAppSecretDto.Id)));
        }

        if (oldAppSecret.AppName != createUpdateAppSecretDto.AppName &&
            await TableWhere(x => x.AppName == createUpdateAppSecretDto.AppName).AnyAsync())
        {
            return OperateResult.Error(ValidationError.IsExist(createUpdateAppSecretDto,
                nameof(createUpdateAppSecretDto.AppName)));
        }

        var appSecret = App.Mapper.MapTo<AppSecret>(createUpdateAppSecretDto);
        var result = await UpdateAsync(appSecret, null, x => new
        {
            x.AppId, x.AppSecretKey
        });
        return OperateResult.Result(result);
    }

    /// <summary>
    /// 删除
    /// </summary>
    /// <param name="ids"></param>
    /// <returns></returns>
    public async Task<OperateResult> DeleteAsync(HashSet<long> ids)
    {
        var appSecrets = await TableWhere(x => ids.Contains(x.Id)).ToListAsync();
        if (appSecrets.Count <= 0)
        {
            return OperateResult.Error(ValidationError.NotExist());
        }

        var result = await LogicDelete<AppSecret>(x => ids.Contains(x.Id));
        return OperateResult.Result(result);
    }

    /// <summary>
    /// 查询
    /// </summary>
    /// <param name="appsecretQueryCriteria"></param>
    /// <param name="pagination"></param>
    /// <returns></returns>
    public async Task<List<AppSecretVo>> QueryAsync(AppsecretQueryCriteria appsecretQueryCriteria,
        Pagination pagination)
    {
        var queryOptions = new QueryOptions<AppSecret>
        {
            Pagination = pagination,
            ConditionalModels = appsecretQueryCriteria.ApplyQueryConditionalModel()
        };
        return App.Mapper.MapTo<List<AppSecretVo>>(
            await TablePageAsync(queryOptions));
    }

    /// <summary>
    /// 下载
    /// </summary>
    /// <param name="appsecretQueryCriteria"></param>
    /// <returns></returns>
    public async Task<List<ExportBase>> DownloadAsync(AppsecretQueryCriteria appsecretQueryCriteria)
    {
        var conditionalModels = appsecretQueryCriteria.ApplyQueryConditionalModel();
        var appSecrets = await TableWhere(conditionalModels).ToListAsync();
        List<ExportBase> appSecretExports = new List<ExportBase>();
        appSecretExports.AddRange(appSecrets.Select(x => new AppSecretExport
        {
            Id = x.Id,
            AppId = x.AppId,
            AppSecretKey = x.AppSecretKey,
            AppName = x.AppName,
            Remark = x.Remark,
            CreateTime = x.CreateTime
        }));
        return appSecretExports;
    }

    #endregion
}
