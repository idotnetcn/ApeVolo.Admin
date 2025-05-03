using System.Collections.Generic;
using System.Threading.Tasks;
using Ape.Volo.Common.Model;
using Ape.Volo.Entity.Core.Permission;
using Ape.Volo.SharedModel.Dto.Core.Permission;
using Ape.Volo.SharedModel.Queries.Common;
using Ape.Volo.SharedModel.Queries.Permission;
using Ape.Volo.ViewModel.Core.Permission;

namespace Ape.Volo.IBusiness.Permission;

/// <summary>
/// apis 接口
/// </summary>
public interface IApisService
{
    /// <summary>
    /// 创建
    /// </summary>
    /// <param name="createUpdateApisDto"></param>
    /// <returns></returns>
    Task<OperateResult> CreateAsync(CreateUpdateApisDto createUpdateApisDto);

    /// <summary>
    /// 更新
    /// </summary>
    /// <param name="createUpdateApisDto"></param>
    /// <returns></returns>
    Task<OperateResult> UpdateAsync(CreateUpdateApisDto createUpdateApisDto);

    /// <summary>
    /// 删除
    /// </summary>
    /// <param name="ids"></param>
    /// <returns></returns>
    Task<OperateResult> DeleteAsync(HashSet<long> ids);

    /// <summary>
    /// 查询
    /// </summary>
    /// <param name="apisQueryCriteria"></param>
    /// <param name="pagination"></param>
    /// <returns></returns>
    Task<List<ApisVo>> QueryAsync(ApisQueryCriteria apisQueryCriteria, Pagination pagination);

    /// <summary>
    /// 查询所有
    /// </summary>
    /// <returns></returns>
    Task<List<ApisVo>> QueryAllAsync();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="apis"></param>
    /// <returns></returns>
    Task<OperateResult> CreateAsync(List<Apis> apis);
}
