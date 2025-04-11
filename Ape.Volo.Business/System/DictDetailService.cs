using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ape.Volo.Business.Base;
using Ape.Volo.Common;
using Ape.Volo.Common.Extensions;
using Ape.Volo.Common.Model;
using Ape.Volo.Entity.System;
using Ape.Volo.IBusiness.Dto.System;
using Ape.Volo.IBusiness.Interface.System;
using Ape.Volo.IBusiness.QueryModel;
using SqlSugar;

namespace Ape.Volo.Business.System;

/// <summary>
/// 字典详情服务
/// </summary>
public class DictDetailService : BaseServices<DictDetail>, IDictDetailService
{
    #region 构造函数

    public DictDetailService()
    {
    }

    #endregion

    #region 基础方法

    public async Task<OperateResult> CreateAsync(CreateUpdateDictDetailDto createUpdateDictDetailDto)
    {
        if (
            await TableWhere(x =>
                x.DictId == createUpdateDictDetailDto.DictId && x.Label == createUpdateDictDetailDto.Label).AnyAsync())
        {
            return OperateResult.Error($"字典标签=>{createUpdateDictDetailDto.Label}=>已存在!");
        }

        if (await TableWhere(x =>
                x.DictId == createUpdateDictDetailDto.DictId && x.Value == createUpdateDictDetailDto.Value).AnyAsync())
        {
            return OperateResult.Error($"字典值=>{createUpdateDictDetailDto.Value}=>已存在!");
        }

        var dictDetail = App.Mapper.MapTo<DictDetail>(createUpdateDictDetailDto);
        var result = await AddAsync(dictDetail);
        return OperateResult.Result(result);
    }

    public async Task<OperateResult> UpdateAsync(CreateUpdateDictDetailDto createUpdateDictDetailDto)
    {
        var oldDictDetail =
            await TableWhere(x => x.Id == createUpdateDictDetailDto.Id).FirstAsync();

        if (oldDictDetail.IsNull())
        {
            return OperateResult.Error("数据不存在！");
        }

        if (oldDictDetail.Label != createUpdateDictDetailDto.Label &&
            await TableWhere(x => x.Label == createUpdateDictDetailDto.Label).AnyAsync())
        {
            return OperateResult.Error($"字典标签=>{createUpdateDictDetailDto.Label}=>已存在!");
        }

        if (oldDictDetail.Value != createUpdateDictDetailDto.Value &&
            await TableWhere(x => x.Value == createUpdateDictDetailDto.Value).AnyAsync())
        {
            return OperateResult.Error($"字典值=>{createUpdateDictDetailDto.Value}=>已存在!");
        }


        var dictDetail = App.Mapper.MapTo<DictDetail>(createUpdateDictDetailDto);
        var result = await UpdateAsync(dictDetail);
        return OperateResult.Result(result);
    }

    public async Task<OperateResult> DeleteAsync(long id)
    {
        var dictDetail = await TableWhere(x => x.Id == id).FirstAsync();
        if (dictDetail == null)
        {
            return OperateResult.Error("数据不存在！");
        }

        var result = await LogicDelete<DictDetail>(x => x.Id == id);
        return OperateResult.Result(result);
    }

    public async Task<List<DictDetailDto>> QueryAsync(DictDetailQueryCriteria dictDetailQueryCriteria,
        Pagination pagination)
    {
        var dictList = await SugarClient.Queryable<Dict>().WithCache(86400).ToListAsync();
        var dictDetailList = await Table.WithCache(86400).ToListAsync();

        if (!dictDetailQueryCriteria.DictName.IsNullOrEmpty())
        {
            var dict = dictList
                .FirstOrDefault(x => x.Name == dictDetailQueryCriteria.DictName);
            if (dict == null)
            {
                return new List<DictDetailDto>();
            }

            dictDetailQueryCriteria.DictId = dict.Id;
        }

        var list = dictDetailList
            .WhereIF(dictDetailQueryCriteria.DictId > 0, x => x.DictId == dictDetailQueryCriteria.DictId)
            .WhereIF(!dictDetailQueryCriteria.Label.IsNullOrEmpty(),
                x => x.Label.Contains(dictDetailQueryCriteria.Label))
            .OrderBy(x => x.DictSort).ToList();

        var pageList = list.Skip((pagination.PageIndex - 1) * pagination.PageSize).Take(pagination.PageSize);
        pagination.TotalElements = list.Count;
        pagination.TotalPages = (int)Math.Ceiling(pagination.TotalElements / (double)pagination.PageSize);
        return App.Mapper.MapTo<List<DictDetailDto>>(pageList);
    }

    #endregion
}
