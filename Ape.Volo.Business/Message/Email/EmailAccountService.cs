using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ape.Volo.Business.Base;
using Ape.Volo.Common;
using Ape.Volo.Common.Extensions;
using Ape.Volo.Common.Global;
using Ape.Volo.Common.Helper;
using Ape.Volo.Common.Model;
using Ape.Volo.Entity.Message.Email;
using Ape.Volo.IBusiness.Dto.Message.Email;
using Ape.Volo.IBusiness.ExportModel.Message.Email.Account;
using Ape.Volo.IBusiness.Interface.Message.Email;
using Ape.Volo.IBusiness.QueryModel;

namespace Ape.Volo.Business.Message.Email;

public class EmailAccountService : BaseServices<EmailAccount>, IEmailAccountService
{
    #region 基础方法

    /// <summary>
    /// 新增
    /// </summary>
    /// <param name="createUpdateEmailAccountDto"></param>
    /// <returns></returns>
    public async Task<OperateResult> CreateAsync(CreateUpdateEmailAccountDto createUpdateEmailAccountDto)
    {
        if (await TableWhere(x => x.Email == createUpdateEmailAccountDto.Email).AnyAsync())
        {
            return OperateResult.Error(DataErrorHelper.IsExist(createUpdateEmailAccountDto,
                nameof(createUpdateEmailAccountDto.Email)));
        }

        var emailAccount = App.Mapper.MapTo<EmailAccount>(createUpdateEmailAccountDto);
        var result = await AddAsync(emailAccount);
        return OperateResult.Result(result);
    }

    /// <summary>
    /// 更新
    /// </summary>
    /// <param name="createUpdateEmailAccountDto"></param>
    /// <returns></returns>
    public async Task<OperateResult> UpdateAsync(CreateUpdateEmailAccountDto createUpdateEmailAccountDto)
    {
        var oldEmailAccount = await TableWhere(x => x.Id == createUpdateEmailAccountDto.Id).FirstAsync();
        if (oldEmailAccount.IsNull())
        {
            return OperateResult.Error(DataErrorHelper.NotExist(createUpdateEmailAccountDto,
                LanguageKeyConstants.EmailAccount,
                nameof(createUpdateEmailAccountDto.Id)));
        }

        if (oldEmailAccount.Email != createUpdateEmailAccountDto.Email &&
            await TableWhere(j => j.Email == createUpdateEmailAccountDto.Email).AnyAsync())
        {
            return OperateResult.Error(DataErrorHelper.IsExist(createUpdateEmailAccountDto,
                nameof(createUpdateEmailAccountDto.Email)));
        }

        var emailAccount = App.Mapper.MapTo<EmailAccount>(createUpdateEmailAccountDto);
        var result = await UpdateAsync(emailAccount);
        return OperateResult.Result(result);
    }

    /// <summary>
    /// 删除
    /// </summary>
    /// <param name="ids"></param>
    /// <returns></returns>
    public async Task<OperateResult> DeleteAsync(HashSet<long> ids)
    {
        var emailAccounts = await TableWhere(x => ids.Contains(x.Id)).ToListAsync();
        if (emailAccounts.Count < 1)
        {
            return OperateResult.Error(DataErrorHelper.NotExist());
        }

        var result = await LogicDelete<EmailAccount>(x => ids.Contains(x.Id));
        return OperateResult.Result(result);
    }

    /// <summary>
    /// 查询
    /// </summary>
    /// <param name="emailAccountQueryCriteria"></param>
    /// <param name="pagination"></param>
    /// <returns></returns>
    public async Task<List<EmailAccountDto>> QueryAsync(EmailAccountQueryCriteria emailAccountQueryCriteria,
        Pagination pagination)
    {
        var queryOptions = new QueryOptions<EmailAccount>
        {
            Pagination = pagination,
            ConditionalModels = emailAccountQueryCriteria.ApplyQueryConditionalModel(),
        };
        return App.Mapper.MapTo<List<EmailAccountDto>>(
            await TablePageAsync(queryOptions));
    }

    public async Task<List<ExportBase>> DownloadAsync(EmailAccountQueryCriteria emailAccountQueryCriteria)
    {
        var emailAccounts = await TableWhere(emailAccountQueryCriteria.ApplyQueryConditionalModel()).ToListAsync();
        List<ExportBase> emailAccountExports = new List<ExportBase>();
        emailAccountExports.AddRange(emailAccounts.Select(x => new EmailAccountExport
        {
            Id = x.Id,
            Email = x.Email,
            DisplayName = x.DisplayName,
            Host = x.Host,
            Port = x.Port,
            Username = x.Username,
            EnableSsl = x.EnableSsl,
            UseDefaultCredentials = x.UseDefaultCredentials,
            CreateTime = x.CreateTime
        }));
        return emailAccountExports;
    }

    #endregion
}
