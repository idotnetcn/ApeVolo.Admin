using System.Collections.Generic;
using System.Threading.Tasks;
using Ape.Volo.Business.Base;
using Ape.Volo.Common;
using Ape.Volo.Common.Attributes;
using Ape.Volo.Common.Extensions;
using Ape.Volo.Common.Global;
using Ape.Volo.Common.Helper;
using Ape.Volo.Common.Model;
using Ape.Volo.Entity.System;
using Ape.Volo.IBusiness.Dto.System;
using Ape.Volo.IBusiness.Interface.System;
using Ape.Volo.IBusiness.QueryModel;

namespace Ape.Volo.Business.System;

/// <summary>
/// 字典详情服务
/// </summary>
public class DictDetailService : BaseServices<DictDetail>, IDictDetailService
{
    #region 基础方法

    public async Task<OperateResult> CreateAsync(CreateUpdateDictDetailDto createUpdateDictDetailDto)
    {
        if (
            await TableWhere(x =>
                x.DictId == createUpdateDictDetailDto.DictId && x.Label == createUpdateDictDetailDto.Label).AnyAsync())
        {
            return OperateResult.Error(DataErrorHelper.IsExist(createUpdateDictDetailDto,
                nameof(createUpdateDictDetailDto.Label)));
        }

        if (await TableWhere(x =>
                x.DictId == createUpdateDictDetailDto.DictId && x.Value == createUpdateDictDetailDto.Value).AnyAsync())
        {
            return OperateResult.Error(DataErrorHelper.IsExist(createUpdateDictDetailDto,
                nameof(createUpdateDictDetailDto.Value)));
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
            return OperateResult.Error(DataErrorHelper.NotExist(createUpdateDictDetailDto,
                LanguageKeyConstants.DictDetail,
                nameof(createUpdateDictDetailDto.Id)));
        }

        if (oldDictDetail.Label != createUpdateDictDetailDto.Label &&
            await TableWhere(x => x.Label == createUpdateDictDetailDto.Label).AnyAsync())
        {
            return OperateResult.Error(DataErrorHelper.IsExist(createUpdateDictDetailDto,
                nameof(createUpdateDictDetailDto.Label)));
        }

        if (oldDictDetail.Value != createUpdateDictDetailDto.Value &&
            await TableWhere(x => x.Value == createUpdateDictDetailDto.Value).AnyAsync())
        {
            return OperateResult.Error(DataErrorHelper.IsExist(createUpdateDictDetailDto,
                nameof(createUpdateDictDetailDto.Value)));
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
            return OperateResult.Error(DataErrorHelper.NotExist());
        }

        var result = await LogicDelete<DictDetail>(x => x.Id == id);
        return OperateResult.Result(result);
    }

    [UseCache(Expiration = 30, KeyPrefix = GlobalConstants.CachePrefix.LoadDictDetailByDictId)]
    public async Task<List<DictDetailDto>> GetDetailByDictIdAsync(long dictId)
    {
        var dictDetail = await TableWhere(x => x.DictId == dictId).OrderBy(x => x.DictSort).ToListAsync();

        return App.Mapper.MapTo<List<DictDetailDto>>(dictDetail);
    }

    public async Task<List<DictDetailDto>> QueryAsync(DictDetailQueryCriteria dictDetailQueryCriteria,
        Pagination pagination)
    {
        if (dictDetailQueryCriteria.DictId > 0)
        {
            dictDetailQueryCriteria.DictName = "";
        }

        if (!dictDetailQueryCriteria.DictName.IsNullOrEmpty())
        {
            var dict = await SugarClient.Queryable<Dict>().Where(x => x.Name == dictDetailQueryCriteria.DictName)
                .FirstAsync();
            if (dict == null)
            {
                return new List<DictDetailDto>();
            }

            var dictDetailList = await App.GetService<IDictDetailService>().GetDetailByDictIdAsync(dict.Id);
            return dictDetailList;
        }

        pagination.SortFields = new List<string> { "dict_sort" };
        var queryOptions = new QueryOptions<DictDetail>
        {
            Pagination = pagination,
            ConditionalModels = dictDetailQueryCriteria.ApplyQueryConditionalModel()
        };
        var list = await TablePageAsync(queryOptions);
        return App.Mapper.MapTo<List<DictDetailDto>>(list);
    }

    #endregion
}
