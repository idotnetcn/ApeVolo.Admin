using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ape.Volo.Business.Base;
using Ape.Volo.Common;
using Ape.Volo.Common.Extensions;
using Ape.Volo.Common.Global;
using Ape.Volo.Common.Helper;
using Ape.Volo.Common.Model;
using Ape.Volo.Entity.System;
using Ape.Volo.IBusiness.Dto.System;
using Ape.Volo.IBusiness.ExportModel.System;
using Ape.Volo.IBusiness.Interface.System;
using Ape.Volo.IBusiness.QueryModel;

namespace Ape.Volo.Business.System;

/// <summary>
/// 字典服务
/// </summary>
public class DictService : BaseServices<Dict>, IDictService
{
    #region 基础方法

    public async Task<OperateResult> CreateAsync(CreateUpdateDictDto createUpdateDictDto)
    {
        if (await TableWhere(d => d.Name == createUpdateDictDto.Name).AnyAsync())
        {
            return OperateResult.Error(DataErrorHelper.IsExist(createUpdateDictDto, nameof(createUpdateDictDto.Name)));
        }

        var result = await AddAsync(App.Mapper.MapTo<Dict>(createUpdateDictDto));
        return OperateResult.Result(result);
    }

    public async Task<OperateResult> UpdateAsync(CreateUpdateDictDto createUpdateDictDto)
    {
        var oldDict =
            await TableWhere(x => x.Id == createUpdateDictDto.Id).FirstAsync();
        if (oldDict.IsNull())
        {
            return OperateResult.Error(DataErrorHelper.NotExist(createUpdateDictDto, LanguageKeyConstants.Dict,
                nameof(createUpdateDictDto.Id)));
        }

        if (oldDict.Name != createUpdateDictDto.Name &&
            await TableWhere(j => j.Name == createUpdateDictDto.Name).AnyAsync())
        {
            return OperateResult.Error(DataErrorHelper.IsExist(createUpdateDictDto, nameof(createUpdateDictDto.Name)));
        }

        var result = await UpdateAsync(App.Mapper.MapTo<Dict>(createUpdateDictDto));
        return OperateResult.Result(result);
    }

    public async Task<OperateResult> DeleteAsync(HashSet<long> ids)
    {
        var dictList = await TableWhere(x => ids.Contains(x.Id)).ToListAsync();
        if (dictList.Count <= 0)
        {
            return OperateResult.Error(DataErrorHelper.NotExist());
        }

        var result = await LogicDelete<Dict>(x => ids.Contains(x.Id));
        return OperateResult.Result(result);
    }

    public async Task<List<DictDto>> QueryAsync(DictQueryCriteria dictQueryCriteria, Pagination pagination)
    {
        var queryOptions = new QueryOptions<Dict>
        {
            Pagination = pagination,
            ConditionalModels = dictQueryCriteria.ApplyQueryConditionalModel()
            //IsIncludes = true
        };
        var list = await TablePageAsync(queryOptions);
        var dicts = App.Mapper.MapTo<List<DictDto>>(list);
        // foreach (var item in dicts)
        // {
        //     item.DictDetails.ForEach(d => d.Dict = new DictDto2 { Id = d.DictId });
        // }

        return dicts;
    }

    public async Task<List<ExportBase>> DownloadAsync(DictQueryCriteria dictQueryCriteria)
    {
        var conditionalModels = dictQueryCriteria.ApplyQueryConditionalModel();
        var dicts = await Table.Includes(x => x.DictDetails).Where(conditionalModels)
            .ToListAsync();
        List<ExportBase> dictExports = new List<ExportBase>();

        dicts.ForEach(x =>
        {
            dictExports.AddRange(x.DictDetails.Select(d => new DictExport
            {
                Id = x.Id,
                DictType = x.DictType,
                Name = x.Name,
                Description = x.Description,
                Lable = d.Label,
                Value = d.Value,
                CreateTime = x.CreateTime
            }));
        });

        return dictExports;
    }

    #endregion
}
