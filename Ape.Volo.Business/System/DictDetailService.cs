using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ape.Volo.Business.Base;
using Ape.Volo.Common;
using Ape.Volo.Common.Exception;
using Ape.Volo.Common.Extensions;
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

    public async Task<bool> CreateAsync(CreateUpdateDictDetailDto createUpdateDictDetailDto)
    {
        if (await TableWhere(dd =>
                dd.DictId == createUpdateDictDetailDto.DictId &&
                (dd.Label == createUpdateDictDetailDto.Label ||
                 dd.Value == createUpdateDictDetailDto.Value)).AnyAsync())
        {
            throw new BadRequestException(
                $"字典详情标签或值=>({createUpdateDictDetailDto.Label},{createUpdateDictDetailDto.Value})=>已存在!");
        }

        var dictDetail = App.Mapper.MapTo<DictDetail>(createUpdateDictDetailDto);
        //dictDetail.DictId = createUpdateDictDetailDto.Dict.Id;
        dictDetail.DictId = createUpdateDictDetailDto.DictId;
        return await AddEntityAsync(dictDetail);
    }

    public async Task<bool> UpdateAsync(CreateUpdateDictDetailDto createUpdateDictDetailDto)
    {
        var oldDictDetail =
            await TableWhere(x => x.Id == createUpdateDictDetailDto.Id).FirstAsync();

        if (oldDictDetail.IsNull())
        {
            throw new BadRequestException("数据不存在！");
        }

        if (oldDictDetail.DictId != createUpdateDictDetailDto.DictId &&
            await TableWhere(dd =>
                dd.DictId == createUpdateDictDetailDto.DictId &&
                (dd.Label == createUpdateDictDetailDto.Label ||
                 dd.Value == createUpdateDictDetailDto.Value)).AnyAsync())
        {
            throw new BadRequestException(
                $"字典详情标签或值=>({createUpdateDictDetailDto.Label},{createUpdateDictDetailDto.Value})=>已存在!");
        }


        var dictDetail = App.Mapper.MapTo<DictDetail>(createUpdateDictDetailDto);
        dictDetail.DictId = createUpdateDictDetailDto.DictId;
        return await UpdateEntityAsync(dictDetail);
    }

    public async Task<bool> DeleteAsync(long id)
    {
        var dictDetail = await TableWhere(x => x.Id == id).FirstAsync();
        if (dictDetail == null)
        {
            throw new BadRequestException("数据不存在！");
        }

        return await LogicDelete<DictDetail>(x => x.Id == id) > 0;
    }

    public async Task<List<DictDetailDto>> QueryAsync(DictDetailQueryCriteria dictDetailQueryCriteria)
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

        return App.Mapper.MapTo<List<DictDetailDto>>(dictDetailList
            .WhereIF(dictDetailQueryCriteria.DictId > 0, x => x.DictId == dictDetailQueryCriteria.DictId)
            .WhereIF(!dictDetailQueryCriteria.Label.IsNullOrEmpty(),
                x => x.Label.Contains(dictDetailQueryCriteria.Label))
            .OrderBy(x => x.DictSort)
            .ToList());
    }

    #endregion
}
