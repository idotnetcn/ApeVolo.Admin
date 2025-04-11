using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ape.Volo.Business.Base;
using Ape.Volo.Common;
using Ape.Volo.Common.Attributes;
using Ape.Volo.Common.Enums;
using Ape.Volo.Common.Exception;
using Ape.Volo.Common.Extensions;
using Ape.Volo.Common.Global;
using Ape.Volo.Common.Model;
using Ape.Volo.Entity.Permission;
using Ape.Volo.IBusiness.Dto.Permission;
using Ape.Volo.IBusiness.ExportModel.Permission;
using Ape.Volo.IBusiness.Interface.Permission;
using Ape.Volo.IBusiness.QueryModel;
using SqlSugar;

namespace Ape.Volo.Business.Permission;

/// <summary>
/// 角色服务
/// </summary>
public class RoleService : BaseServices<Role>, IRoleService
{
    #region 字段

    #endregion

    #region 构造函数

    public RoleService()
    {
    }

    #endregion

    #region 基础方法

    [UseTran]
    public async Task<OperateResult> CreateAsync(CreateUpdateRoleDto createUpdateRoleDto)
    {
        await VerificationUserRoleLevelAsync(createUpdateRoleDto.Level);
        if (await TableWhere(r => r.Name == createUpdateRoleDto.Name).AnyAsync())
        {
            return OperateResult.Error($"角色名称=>{createUpdateRoleDto.Name}=>已存在!");
        }

        if (await TableWhere(r => r.Permission == createUpdateRoleDto.Permission).AnyAsync())
        {
            return OperateResult.Error($"权限标识=>{createUpdateRoleDto.Permission}=>已存在!");
        }

        if (createUpdateRoleDto.DataScopeType == DataScopeType.Customize && createUpdateRoleDto.Depts.Count == 0)
        {
            return OperateResult.Error("数据权限为自定义,请至少选择一个部门!");
        }

        var role = App.Mapper.MapTo<Role>(createUpdateRoleDto);
        await AddAsync(role);

        if (createUpdateRoleDto.DataScopeType == DataScopeType.Customize && createUpdateRoleDto.Depts.Count != 0)
        {
            var roleDepts = new List<RoleDepartment>();
            roleDepts.AddRange(createUpdateRoleDto.Depts.Select(rd => new RoleDepartment
                { RoleId = role.Id, DeptId = rd.Id }));
            await SugarClient.Insertable(roleDepts).ExecuteCommandAsync();
        }

        return OperateResult.Success();
    }

    [UseTran]
    public async Task<OperateResult> UpdateAsync(CreateUpdateRoleDto createUpdateRoleDto)
    {
        //取出待更新数据
        var oldRole = await TableWhere(x => x.Id == createUpdateRoleDto.Id).Includes(x => x.Users).FirstAsync();
        if (oldRole.IsNull())
        {
            return OperateResult.Error("数据不存在！");
        }

        if (oldRole.Name != createUpdateRoleDto.Name &&
            await TableWhere(x => x.Name == createUpdateRoleDto.Name).AnyAsync())
        {
            return OperateResult.Error($"角色名称=>{createUpdateRoleDto.Name}=>已存在!");
        }

        if (oldRole.Permission != createUpdateRoleDto.Permission &&
            await TableWhere(x => x.Permission == createUpdateRoleDto.Permission).AnyAsync())
        {
            return OperateResult.Error($"权限标识=>{createUpdateRoleDto.Permission}=>已存在!");
        }

        await VerificationUserRoleLevelAsync(createUpdateRoleDto.Level);
        var role = App.Mapper.MapTo<Role>(createUpdateRoleDto);
        await UpdateAsync(role);

        //删除部门权限关联
        await SugarClient.Deleteable<RoleDepartment>().Where(x => x.RoleId == role.Id).ExecuteCommandAsync();
        if (!createUpdateRoleDto.Depts.IsNullOrEmpty() && createUpdateRoleDto.Depts.Count > 0)
        {
            var roleDepts = new List<RoleDepartment>();
            roleDepts.AddRange(createUpdateRoleDto.Depts.Select(rd => new RoleDepartment
                { RoleId = role.Id, DeptId = rd.Id }));
            await SugarClient.Insertable(roleDepts).ExecuteCommandAsync();
        }

        foreach (var user in oldRole.Users)
        {
            await App.Cache.RemoveAsync(GlobalConstants.CachePrefix.UserDataScopeById +
                                        user.Id.ToString().ToMd5String16());
        }

        return OperateResult.Success();
    }

    [UseTran]
    public async Task<OperateResult> DeleteAsync(HashSet<long> ids)
    {
        //返回用户列表的最大角色等级
        var roles = await TableWhere(x => ids.Contains(x.Id)).Includes(x => x.Users).ToListAsync();
        int userCount = 0;
        if (roles.Any(role => role.Users != null && role.Users.Count != 0))
        {
            userCount++;
        }

        if (userCount > 0)
        {
            return OperateResult.Error("存在用户关联，请解除后再试！");
        }


        List<int> levels = new List<int>();
        levels.AddRange(roles.Select(x => x.Level).ToList());
        int minLevel = levels.Min();
        //验证当前用户角色等级是否大于待待删除的角色等级 ，不满足则抛异常
        await VerificationUserRoleLevelAsync(minLevel);

        //删除角色 角色部门 角色菜单
        var result = await LogicDelete<Role>(x => ids.Contains(x.Id));
        return OperateResult.Result(result);
    }

    public async Task<List<RoleDto>> QueryAsync(RoleQueryCriteria roleQueryCriteria, Pagination pagination)
    {
        var queryOptions = new QueryOptions<Role>
        {
            Pagination = pagination,
            ConditionalModels = roleQueryCriteria.ApplyQueryConditionalModel(),
            IsIncludes = true,
            IgnorePropertyNameList = new[] { "Users" }
        };
        var roleList =
            await TablePageAsync(queryOptions);

        return App.Mapper.MapTo<List<RoleDto>>(roleList);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="roleQueryCriteria"></param>
    /// <returns></returns>
    public async Task<List<ExportBase>> DownloadAsync(RoleQueryCriteria roleQueryCriteria)
    {
        var roles = await TableWhere(roleQueryCriteria.ApplyQueryConditionalModel()).Includes(x => x.DepartmentList)
            .ToListAsync();
        List<ExportBase> roleExports = new List<ExportBase>();
        roleExports.AddRange(roles.Select(x => new RoleExport()
        {
            Id = x.Id,
            Name = x.Name,
            Level = x.Level,
            Description = x.Description,
            DataScopeType = x.DataScopeType,
            DataDept = string.Join(",", x.DepartmentList.Select(d => d.Name).ToArray()),
            Permission = x.Permission,
            CreateTime = x.CreateTime
        }));
        return roleExports;
    }

    #endregion

    #region 扩展方法

    public async Task<List<RoleDto>> QueryAllAsync()
    {
        var roleList = await Table.Includes(x => x.MenuList).Includes(x => x.DepartmentList).ToListAsync();

        return App.Mapper.MapTo<List<RoleDto>>(roleList);
    }

    public async Task<int?> QueryUserRoleLevelAsync(HashSet<long> ids)
    {
        var levels = await SugarClient.Queryable<Role, UserRole>((r, ur) => new JoinQueryInfos(
                JoinType.Left, r.Id == ur.RoleId
            )).Where((r, ur) => ids.Contains(ur.UserId))
            .Select((r) => r.Level).ToListAsync();
        if (levels.Any())
        {
            var minLevel = levels.Min();
            return minLevel;
        }

        return null;
    }

    public async Task<int> VerificationUserRoleLevelAsync(int? level)
    {
        var minLevel = 999;
        var levels = await SugarClient.Queryable<Role, UserRole>((r, ur) => new JoinQueryInfos(
                JoinType.Left, r.Id == ur.RoleId
            )).Where((r, ur) => ur.UserId == App.HttpUser.Id)
            .Select((r) => r.Level).ToListAsync();

        if (levels.Any())
        {
            minLevel = levels.Min();
        }

        if (level != null && level < minLevel)
        {
            throw new BadRequestException("您无权修改或删除比你角色等级更高的数据！");
        }

        return minLevel;
    }


    [UseTran]
    public async Task<OperateResult> UpdateRolesMenusAsync(CreateUpdateRoleDto createUpdateRoleDto)
    {
        var role = await TableWhere(x => x.Id == createUpdateRoleDto.Id).Includes(x => x.Users).FirstAsync();
        await VerificationUserRoleLevelAsync(role.Level);


        //删除菜单
        List<RoleMenu> roleMenus = new List<RoleMenu>();
        if (!createUpdateRoleDto.Menus.IsNullOrEmpty() && createUpdateRoleDto.Menus.Count > 0)
        {
            roleMenus.AddRange(createUpdateRoleDto.Menus.Select(rm => new RoleMenu
                { RoleId = role.Id, MenuId = rm.Id }));

            await SugarClient.Deleteable<RoleMenu>().Where(x => x.RoleId == role.Id).ExecuteCommandAsync();
            await SugarClient.Insertable(roleMenus).ExecuteCommandAsync();
        }

        //删除用户缓存
        foreach (var user in role.Users)
        {
            await App.Cache.RemoveAsync(GlobalConstants.CachePrefix.UserPermissionRoles +
                                        user.Id.ToString().ToMd5String16());
            await App.Cache.RemoveAsync(GlobalConstants.CachePrefix.UserMenuById +
                                        user.Id.ToString().ToMd5String16());
        }

        return OperateResult.Success();
    }


    [UseTran]
    public async Task<OperateResult> UpdateRolesApisAsync(CreateUpdateRoleDto createUpdateRoleDto)
    {
        var role = await TableWhere(x => x.Id == createUpdateRoleDto.Id).Includes(x => x.Users).FirstAsync();
        await VerificationUserRoleLevelAsync(role.Level);


        //删除菜单
        List<RoleApis> roleApis = new List<RoleApis>();
        if (createUpdateRoleDto.Apis.Any())
        {
            // 这里过滤一下自生成的一级节点ID
            createUpdateRoleDto.Apis = createUpdateRoleDto.Apis.Where(x => x.Id > 10000).ToList();
            roleApis.AddRange(createUpdateRoleDto.Apis.Select(ra => new RoleApis()
                { RoleId = role.Id, ApisId = ra.Id }));

            await SugarClient.Deleteable<RoleApis>().Where(x => x.RoleId == role.Id).ExecuteCommandAsync();
            await SugarClient.Insertable(roleApis).ExecuteCommandAsync();


            //删除用户缓存
            foreach (var user in role.Users)
            {
                await App.Cache.RemoveAsync(GlobalConstants.CachePrefix.UserPermissionUrls +
                                            user.Id.ToString().ToMd5String16());
                await App.Cache.RemoveAsync(GlobalConstants.CachePrefix.UserMenuById +
                                            user.Id.ToString().ToMd5String16());
            }
        }

        return OperateResult.Success();
    }

    #endregion
}
