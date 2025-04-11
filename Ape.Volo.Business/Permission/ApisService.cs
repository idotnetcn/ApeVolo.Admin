using System.Collections.Generic;
using System.Threading.Tasks;
using Ape.Volo.Business.Base;
using Ape.Volo.Common;
using Ape.Volo.Common.Extensions;
using Ape.Volo.Common.Model;
using Ape.Volo.Entity.Permission;
using Ape.Volo.IBusiness.Dto.Permission;
using Ape.Volo.IBusiness.Interface.Permission;
using Ape.Volo.IBusiness.QueryModel;

namespace Ape.Volo.Business.Permission;

public class ApisService : BaseServices<Apis>, IApisService
{
    public ApisService()
    {
    }

    public async Task<OperateResult> CreateAsync(CreateUpdateApisDto createUpdateApisDto)
    {
        if (await TableWhere(a => a.Url == createUpdateApisDto.Url).AnyAsync())
        {
            return OperateResult.Error($"Url=>{createUpdateApisDto.Url}=>已存在!");
        }

        var apis = App.Mapper.MapTo<Apis>(createUpdateApisDto);
        var result = await AddAsync(apis);
        return OperateResult.Result(result);
    }

    public async Task<OperateResult> UpdateAsync(CreateUpdateApisDto createUpdateApisDto)
    {
        var oldApis =
            await TableWhere(x => x.Id == createUpdateApisDto.Id).FirstAsync();
        if (oldApis.IsNull())
        {
            return OperateResult.Error("数据不存在！");
        }

        if (oldApis.Url != createUpdateApisDto.Url &&
            await TableWhere(a => a.Url == createUpdateApisDto.Url).AnyAsync())
        {
            return OperateResult.Error($"Url=>{createUpdateApisDto.Url}=>已存在!");
        }

        var apis = App.Mapper.MapTo<Apis>(createUpdateApisDto);
        var result = await UpdateAsync(apis);
        return OperateResult.Result(result);
    }

    public async Task<OperateResult> DeleteAsync(HashSet<long> ids)
    {
        var apis = await TableWhere(x => ids.Contains(x.Id)).ToListAsync();
        if (apis.Count < 1)
        {
            return OperateResult.Error("数据不存在！");
        }

        var result = await LogicDelete<Apis>(x => ids.Contains(x.Id));
        return OperateResult.Result(result);
    }

    public async Task<List<Apis>> QueryAsync(ApisQueryCriteria apisQueryCriteria, Pagination pagination)
    {
        var queryOptions = new QueryOptions<Apis>
        {
            Pagination = pagination,
            ConditionalModels = apisQueryCriteria.ApplyQueryConditionalModel()
        };
        return await TablePageAsync(queryOptions);
    }

    public async Task<List<Apis>> QueryAllAsync()
    {
        return await Table.ToListAsync();
    }

    public async Task<OperateResult> CreateAsync(List<Apis> apis)
    {
        var result = await AddAsync(apis);
        return OperateResult.Result(result);
    }
}
