using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ape.Volo.Business.Base;
using Ape.Volo.Common;
using Ape.Volo.Common.Attributes;
using Ape.Volo.Common.Extensions;
using Ape.Volo.Common.Global;
using Ape.Volo.Common.Helper;
using Ape.Volo.Common.Model;
using Ape.Volo.Entity.Permission;
using Ape.Volo.IBusiness.Dto.Permission;
using Ape.Volo.IBusiness.ExportModel.Permission;
using Ape.Volo.IBusiness.Interface.Permission;
using Ape.Volo.IBusiness.QueryModel;

namespace Ape.Volo.Business.Permission;

public class DepartmentService : BaseServices<Department>, IDepartmentService
{
    #region 基础方法

    [UseTran]
    public async Task<OperateResult> CreateAsync(CreateUpdateDepartmentDto createUpdateDepartmentDto)
    {
        if (await TableWhere(d => d.Name == createUpdateDepartmentDto.Name).AnyAsync())
        {
            return OperateResult.Error(DataErrorHelper.IsExist(createUpdateDepartmentDto,
                nameof(createUpdateDepartmentDto.Name)));
        }

        Department dept =
            App.Mapper.MapTo<Department>(createUpdateDepartmentDto);
        await AddAsync(dept);

        //重新计算子节点个数
        if (dept.ParentId != 0)
        {
            var department = await TableWhere(x => x.Id == dept.ParentId).FirstAsync();
            if (department.IsNotNull())
            {
                var count = await SugarClient.Queryable<Department>().Where(x => x.ParentId == department.Id)
                    .CountAsync();
                department.SubCount = count;

                await UpdateAsync(department);
            }
        }

        return OperateResult.Success();
    }

    [UseTran]
    public async Task<OperateResult> UpdateAsync(CreateUpdateDepartmentDto createUpdateDepartmentDto)
    {
        var oldUseDepartment =
            await TableWhere(x => x.Id == createUpdateDepartmentDto.Id).FirstAsync();
        if (oldUseDepartment.IsNull())
        {
            return OperateResult.Error(DataErrorHelper.NotExist(createUpdateDepartmentDto,
                LanguageKeyConstants.Department,
                nameof(createUpdateDepartmentDto.Id)));
        }

        if (oldUseDepartment.Name != createUpdateDepartmentDto.Name &&
            await TableWhere(x => x.Name == createUpdateDepartmentDto.Name).AnyAsync())
        {
            return OperateResult.Error(DataErrorHelper.IsExist(createUpdateDepartmentDto,
                nameof(createUpdateDepartmentDto.Name)));
        }

        Department dept =
            App.Mapper.MapTo<Department>(createUpdateDepartmentDto);
        dept.SubCount = oldUseDepartment.SubCount;
        await UpdateAsync(dept);

        //重新计算子节点个数
        //判断修改前父部门是否与修改后相同  如果相同说明并没有修改上下级部门信息
        if (oldUseDepartment.ParentId != dept.ParentId)
        {
            if (dept.ParentId != 0)
            {
                var department = await TableWhere(x => x.Id == dept.ParentId).FirstAsync();
                if (department.IsNotNull())
                {
                    var count = await SugarClient.Queryable<Department>().Where(x => x.ParentId == department.Id)
                        .CountAsync();
                    department.SubCount = count;
                    await UpdateAsync(department, x => x.SubCount);
                }
            }

            if (oldUseDepartment.ParentId != 0)
            {
                var department =
                    await TableWhere(x => x.Id == oldUseDepartment.ParentId).FirstAsync();
                if (department.IsNotNull())
                {
                    var count = await SugarClient.Queryable<Department>().Where(x => x.ParentId == department.Id)
                        .CountAsync();
                    department.SubCount = count;
                    await UpdateAsync(department, x => x.SubCount);
                }
            }
        }

        return OperateResult.Success();
    }

    [UseTran]
    public async Task<OperateResult> DeleteAsync(List<long> ids)
    {
        var allIds = await GetChildIds(ids, null);
        var departmentList = await TableWhere(x => allIds.Contains(x.Id)).Includes(x => x.Users).Includes(x => x.Roles)
            .ToListAsync();
        if (departmentList.Count < 1)
        {
            return OperateResult.Error(DataErrorHelper.NotExist());
        }

        if (departmentList.Any(dept => dept.Users != null && dept.Users.Count != 0))
        {
            return OperateResult.Error(DataErrorHelper.DataAssociationExists());
        }

        if (departmentList.Any(dept => dept.Roles != null && dept.Roles.Count != 0))
        {
            return OperateResult.Error(DataErrorHelper.DataAssociationExists());
        }


        var pIds = departmentList.Select(x => x.ParentId);

        var updateDepartmentList = await TableWhere(x => pIds.Contains(x.Id)).ToListAsync();

        var isTrue = await LogicDelete<Department>(x => allIds.Contains(x.Id));

        if (isTrue)
        {
            if (updateDepartmentList.Any())
            {
                foreach (var d in updateDepartmentList)
                {
                    var count = await SugarClient.Queryable<Department>().Where(x => x.ParentId == d.Id)
                        .CountAsync();
                    d.SubCount = count;
                }

                isTrue = await UpdateAsync(updateDepartmentList, x => x.SubCount);
            }
        }

        return OperateResult.Result(isTrue);
    }


    public async Task<List<DepartmentDto>> QueryAsync(DeptQueryCriteria deptQueryCriteria,
        Pagination pagination)
    {
        List<Department> deptList;
        if (deptQueryCriteria.ParentId == 0)
        {
            var queryOptions = new QueryOptions<Department>
            {
                Pagination = pagination,
                ConditionalModels = deptQueryCriteria.ApplyQueryConditionalModel()
            };
            deptList = await TablePageAsync(queryOptions);
        }
        else
        {
            deptList = await TableWhere(deptQueryCriteria.ApplyQueryConditionalModel()).ToListAsync();
        }

        var deptDataList = App.Mapper.MapTo<List<DepartmentDto>>(deptList);

        pagination.TotalElements = deptDataList.Count;
        return deptDataList;
    }

    public async Task<List<DepartmentDto>> QueryAllAsync()
    {
        var deptList = App.Mapper.MapTo<List<DepartmentDto>>(await Table.ToListAsync());
        return deptList;
    }


    public async Task<List<ExportBase>> DownloadAsync(DeptQueryCriteria deptQueryCriteria)
    {
        var depts = await TableWhere(deptQueryCriteria.ApplyQueryConditionalModel()).ToListAsync();
        List<ExportBase> roleExports = new List<ExportBase>();
        roleExports.AddRange(depts.Select(x => new DepartmentExport
        {
            Id = x.Id,
            Name = x.Name,
            ParentId = x.ParentId,
            Sort = x.Sort,
            Enabled = x.Enabled,
            SubCount = x.SubCount,
            CreateTime = x.CreateTime
        }));
        return roleExports;
    }

    #endregion

    #region 扩展方法

    public async Task<List<DepartmentDto>> QuerySuperiorDeptAsync(long id)
    {
        var departmentList = new List<DepartmentDto>();
        var dept = await TableWhere(x => x.Id == id).FirstAsync();
        var deptDto = App.Mapper.MapTo<DepartmentDto>(dept);
        var departmentDtoList = await FindSuperiorAsync(deptDto, new List<DepartmentDto>());
        departmentList.AddRange(departmentDtoList);

        departmentList = TreeHelper<DepartmentDto>.ListToTrees(departmentList, "Id", "ParentId", 0);

        return departmentList;
    }

    public async Task<List<DepartmentDto>> QueryByPIdAsync(long id)
    {
        return App.Mapper.MapTo<List<DepartmentDto>>(await TableWhere(x =>
            x.ParentId == id && x.Enabled).ToListAsync());
    }

    public async Task<DepartmentSmallDto> QueryByIdAsync(long id)
    {
        return App.Mapper.MapTo<DepartmentSmallDto>(await TableWhere(x =>
            x.Id == id && x.Enabled).FirstAsync());
    }

    #endregion

    #region 私有方法

    /// <summary>
    /// 获取顶级部门
    /// </summary>
    /// <returns></returns>
    private async Task<List<DepartmentDto>> FindByPIdIsNullAsync()
    {
        return App.Mapper.MapTo<List<DepartmentDto>>(
            await TableWhere(x => x.ParentId == 0 && x.Enabled).ToListAsync());
    }

    /// <summary>
    /// 查找同级和所有上级部门
    /// </summary>
    /// <param name="departmentDto"></param>
    /// <param name="departmentDtoList"></param>
    /// <returns></returns>
    private async Task<List<DepartmentDto>> FindSuperiorAsync(DepartmentDto departmentDto,
        List<DepartmentDto> departmentDtoList)
    {
        while (true)
        {
            if (departmentDto.ParentId == 0)
            {
                departmentDtoList.AddRange(await FindByPIdIsNullAsync());
                return departmentDtoList;
            }

            departmentDtoList.AddRange(await QueryByPIdAsync(Convert.ToInt64(departmentDto.ParentId)));
            var dto = departmentDto;
            departmentDto =
                App.Mapper.MapTo<DepartmentDto>(await TableWhere(x => x.Id == dto.ParentId)
                    .FirstAsync());
        }
    }

    /// <summary>
    /// 获取所选部门及全部下级部门ID
    /// </summary>
    /// <param name="ids"></param>
    /// <param name="allIds"></param>
    /// <returns></returns>
    public async Task<List<long>> GetChildIds(List<long> ids, List<long> allIds)
    {
        allIds ??= new List<long>();

        foreach (var id in ids.Where(id => !allIds.Contains(id)))
        {
            allIds.Add(id);
            var list = await TableWhere(x => x.ParentId == id && x.Enabled).ToListAsync();
            if (list.Any())
            {
                await GetChildIds(list.Select(x => x.Id).ToList(), allIds);
            }
        }

        return allIds;
    }

    #endregion
}
