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
/// 租户接口
/// </summary>
public interface ITenantService : IBaseServices<Tenant>
{
    #region 基础接口

    /// <summary>
    /// 创建
    /// </summary>
    /// <param name="createUpdateTenantDtoDto"></param>
    /// <returns></returns>
    Task<OperateResult> CreateAsync(CreateUpdateTenantDto createUpdateTenantDtoDto);

    /// <summary>
    /// 更新
    /// </summary>
    /// <param name="createUpdateTenantDtoDto"></param>
    /// <returns></returns>
    Task<OperateResult> UpdateAsync(CreateUpdateTenantDto createUpdateTenantDtoDto);

    /// <summary>
    /// 删除
    /// </summary>
    /// <param name="ids"></param>
    /// <returns></returns>
    Task<OperateResult> DeleteAsync(HashSet<long> ids);

    /// <summary>
    /// 查询
    /// </summary>
    /// <param name="tenantQueryCriteria"></param>
    /// <param name="pagination"></param>
    /// <returns></returns>
    Task<List<TenantVo>> QueryAsync(TenantQueryCriteria tenantQueryCriteria, Pagination pagination);


    /// <summary>
    /// 查询全部
    /// </summary>
    /// <returns></returns>
    Task<List<TenantVo>> QueryAllAsync();

    /// <summary>
    /// 下载
    /// </summary>
    /// <param name="tenantQueryCriteria"></param>
    /// <returns></returns>
    Task<List<ExportBase>> DownloadAsync(TenantQueryCriteria tenantQueryCriteria);

    #endregion
}
