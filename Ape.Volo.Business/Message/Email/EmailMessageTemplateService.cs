using System.Collections.Generic;
using System.Threading.Tasks;
using Ape.Volo.Common.Extensions;
using Ape.Volo.Common.Global;
using Ape.Volo.Common.Model;
using Ape.Volo.Core;
using Ape.Volo.Core.Utils;
using Ape.Volo.Entity.Core.Message.Email;
using Ape.Volo.IBusiness.Message.Email;
using Ape.Volo.SharedModel.Dto.Core.Message.Email;
using Ape.Volo.SharedModel.Queries.Common;
using Ape.Volo.SharedModel.Queries.Message;
using Ape.Volo.ViewModel.Core.Message.Email;

namespace Ape.Volo.Business.Message.Email;

/// <summary>
/// 邮件消息模板实现
/// </summary>
public class EmailMessageTemplateService : BaseServices<EmailMessageTemplate>, IEmailMessageTemplateService
{
    #region 基础方法

    /// <summary>
    /// 新增
    /// </summary>
    /// <param name="createUpdateEmailMessageTemplateDto"></param>
    /// <returns></returns>
    public async Task<OperateResult> CreateAsync(
        CreateUpdateEmailMessageTemplateDto createUpdateEmailMessageTemplateDto)
    {
        if (await TableWhere(x => x.Name == createUpdateEmailMessageTemplateDto.Name).AnyAsync())
        {
            return OperateResult.Error(ValidationError.IsExist(createUpdateEmailMessageTemplateDto,
                nameof(createUpdateEmailMessageTemplateDto.Name)));
        }


        var result = await AddAsync(App.Mapper.MapTo<EmailMessageTemplate>(createUpdateEmailMessageTemplateDto));
        return OperateResult.Result(result);
    }

    /// <summary>
    /// 修改
    /// </summary>
    /// <param name="createUpdateEmailMessageTemplateDto"></param>
    /// <returns></returns>
    public async Task<OperateResult> UpdateAsync(
        CreateUpdateEmailMessageTemplateDto createUpdateEmailMessageTemplateDto)
    {
        var emailMessageTemplate = await TableWhere(x => x.Id == createUpdateEmailMessageTemplateDto.Id).FirstAsync();
        if (emailMessageTemplate.IsNull())
        {
            return OperateResult.Error(ValidationError.NotExist(createUpdateEmailMessageTemplateDto,
                LanguageKeyConstants.EmailMessageTemplate,
                nameof(createUpdateEmailMessageTemplateDto.Id)));
        }

        if (emailMessageTemplate.Name != createUpdateEmailMessageTemplateDto.Name &&
            await TableWhere(j => j.Name == emailMessageTemplate.Name).AnyAsync())
        {
            return OperateResult.Error(ValidationError.IsExist(createUpdateEmailMessageTemplateDto,
                nameof(createUpdateEmailMessageTemplateDto.Name)));
        }

        var result = await UpdateAsync(
            App.Mapper.MapTo<EmailMessageTemplate>(createUpdateEmailMessageTemplateDto));
        return OperateResult.Result(result);
    }

    /// <summary>
    /// 删除
    /// </summary>
    /// <param name="ids"></param>
    /// <returns></returns>
    public async Task<OperateResult> DeleteAsync(HashSet<long> ids)
    {
        var messageTemplateList = await TableWhere(x => ids.Contains(x.Id)).ToListAsync();
        if (messageTemplateList.Count <= 0)
        {
            return OperateResult.Error(ValidationError.NotExist());
        }

        var result = await LogicDelete<EmailMessageTemplate>(x => ids.Contains(x.Id));
        return OperateResult.Result(result);
    }

    /// <summary>
    /// 查询
    /// </summary>
    /// <param name="messageTemplateQueryCriteria"></param>
    /// <param name="pagination"></param>
    /// <returns></returns>
    public async Task<List<EmailMessageTemplateVo>> QueryAsync(
        EmailMessageTemplateQueryCriteria messageTemplateQueryCriteria, Pagination pagination)
    {
        var queryOptions = new QueryOptions<EmailMessageTemplate>
        {
            Pagination = pagination,
            ConditionalModels = messageTemplateQueryCriteria.ApplyQueryConditionalModel(),
        };
        return App.Mapper.MapTo<List<EmailMessageTemplateVo>>(
            await TablePageAsync(queryOptions));
    }

    #endregion
}
