using System.Collections.Generic;
using System.Threading.Tasks;
using Ape.Volo.Common.Model;
using Ape.Volo.Entity.Core.Permission;
using Ape.Volo.SharedModel.Dto.Core.Permission;
using Ape.Volo.SharedModel.Queries.Permission;
using Ape.Volo.ViewModel.Core.Permission.Menu;

namespace Ape.Volo.IBusiness.Permission;

/// <summary>
/// 菜单接口
/// </summary>
public interface IMenuService : IBaseServices<Menu>
{
    #region 基础接口

    /// <summary>
    /// 创建
    /// </summary>
    /// <param name="createUpdateMenuDto"></param>
    /// <returns></returns>
    Task<OperateResult> CreateAsync(CreateUpdateMenuDto createUpdateMenuDto);

    /// <summary>
    /// 更新
    /// </summary>
    /// <param name="createUpdateMenuDto"></param>
    /// <returns></returns>
    Task<OperateResult> UpdateAsync(CreateUpdateMenuDto createUpdateMenuDto);

    /// <summary>
    /// 删除
    /// </summary>
    /// <param name="ids"></param>
    /// <returns></returns>
    Task<OperateResult> DeleteAsync(HashSet<long> ids);

    /// <summary>
    /// 查询
    /// </summary>
    /// <param name="menuQueryCriteria"></param>
    /// <returns></returns>
    Task<List<MenuVo>> QueryAsync(MenuQueryCriteria menuQueryCriteria);

    /// <summary>
    /// 下载
    /// </summary>
    /// <param name="menuQueryCriteria"></param>
    /// <returns></returns>
    Task<List<ExportBase>> DownloadAsync(MenuQueryCriteria menuQueryCriteria);

    #endregion

    #region 扩展接口

    /// <summary>
    /// 获取所有菜单
    /// </summary>
    /// <returns></returns>
    Task<List<MenuVo>> QueryAllAsync();

    /// <summary>
    /// 根据父ID获取菜单
    /// </summary>
    /// <param name="pid"></param>
    /// <returns></returns>
    Task<List<MenuVo>> FindByPIdAsync(long pid = 0);


    /// <summary>
    /// 构建前端菜单树
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <returns></returns>
    Task<List<MenuTreeVo>> BuildTreeAsync(long userId);

    /// <summary>
    /// 获取所有同级或父级菜单
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task<List<MenuVo>> FindSuperiorAsync(long id);

    /// <summary>
    /// 获取所有子菜单ID
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task<List<long>> FindChildAsync(long id);

    #endregion
}
