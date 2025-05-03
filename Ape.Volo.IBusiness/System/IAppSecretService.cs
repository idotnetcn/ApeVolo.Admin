using System.Collections.Generic;
using System.Threading.Tasks;
using Ape.Volo.Common.Model;
using Ape.Volo.Entity.Core.System;
using Ape.Volo.SharedModel.Dto.Core.System;
using Ape.Volo.SharedModel.Queries.Common;
using Ape.Volo.SharedModel.Queries.System;
using Ape.Volo.ViewModel.Core.System;

namespace Ape.Volo.IBusiness.System;

/// <summary>
/// 应用秘钥
/// </summary>
public interface IAppSecretService : IBaseServices<AppSecret>
{
    #region 基础接口

    /// <summary>
    /// 创建
    /// </summary>
    /// <param name="createUpdateAppSecretDto"></param>
    /// <returns></returns>
    Task<OperateResult> CreateAsync(CreateUpdateAppSecretDto createUpdateAppSecretDto);

    /// <summary>
    /// 更新
    /// </summary>
    /// <param name="createUpdateAppSecretDto"></param>
    /// <returns></returns>
    Task<OperateResult> UpdateAsync(CreateUpdateAppSecretDto createUpdateAppSecretDto);

    /// <summary>
    /// 删除
    /// </summary>
    /// <param name="ids"></param>
    /// <returns></returns>
    Task<OperateResult> DeleteAsync(HashSet<long> ids);

    /// <summary>
    /// 查询
    /// </summary>
    /// <param name="appsecretQueryCriteria"></param>
    /// <param name="pagination"></param>
    /// <returns></returns>
    Task<List<AppSecretVo>> QueryAsync(AppsecretQueryCriteria appsecretQueryCriteria, Pagination pagination);

    /// <summary>
    /// 下载
    /// </summary>
    /// <param name="appsecretQueryCriteria"></param>
    /// <returns></returns>
    Task<List<ExportBase>> DownloadAsync(AppsecretQueryCriteria appsecretQueryCriteria);

    #endregion
}
