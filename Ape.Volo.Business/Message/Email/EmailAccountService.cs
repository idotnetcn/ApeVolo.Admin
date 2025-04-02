using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ape.Volo.Business.Base;
using Ape.Volo.Common;
using Ape.Volo.Common.Exception;
using Ape.Volo.Common.Extensions;
using Ape.Volo.Common.Global;
using Ape.Volo.Common.Model;
using Ape.Volo.Entity.Message.Email;
using Ape.Volo.IBusiness.Dto.Message.Email;
using Ape.Volo.IBusiness.ExportModel.Message.Email.Account;
using Ape.Volo.IBusiness.Interface.Message.Email;
using Ape.Volo.IBusiness.QueryModel;

namespace Ape.Volo.Business.Message.Email;

public class EmailAccountService : BaseServices<EmailAccount>, IEmailAccountService
{
    #region 构造函数

    public EmailAccountService()
    {
    }

    #endregion

    #region 基础方法

    /// <summary>
    /// 新增
    /// </summary>
    /// <param name="createUpdateEmailAccountDto"></param>
    /// <returns></returns>
    public async Task<bool> CreateAsync(CreateUpdateEmailAccountDto createUpdateEmailAccountDto)
    {
        if (await TableWhere(x => x.Email == createUpdateEmailAccountDto.Email).AnyAsync())
        {
            throw new BadRequestException($"邮箱账户=>{createUpdateEmailAccountDto.Email}=>已存在!");
        }

        var emailAccount = App.Mapper.MapTo<EmailAccount>(createUpdateEmailAccountDto);
        return await AddEntityAsync(emailAccount);
    }

    /// <summary>
    /// 更新
    /// </summary>
    /// <param name="createUpdateEmailAccountDto"></param>
    /// <returns></returns>
    public async Task<bool> UpdateAsync(CreateUpdateEmailAccountDto createUpdateEmailAccountDto)
    {
        var oldEmailAccount = await TableWhere(x => x.Id == createUpdateEmailAccountDto.Id).FirstAsync();
        if (oldEmailAccount.IsNull())
        {
            throw new BadRequestException("数据不存在！");
        }

        if (oldEmailAccount.Email != createUpdateEmailAccountDto.Email &&
            await TableWhere(j => j.Email == createUpdateEmailAccountDto.Email).AnyAsync())
        {
            throw new BadRequestException($"邮箱账户=>{createUpdateEmailAccountDto.Email}=>已存在!");
        }

        var emailAccount = App.Mapper.MapTo<EmailAccount>(createUpdateEmailAccountDto);
        return await UpdateEntityAsync(emailAccount);
    }

    /// <summary>
    /// 删除
    /// </summary>
    /// <param name="ids"></param>
    /// <returns></returns>
    public async Task<bool> DeleteAsync(HashSet<long> ids)
    {
        var emailAccounts = await TableWhere(x => ids.Contains(x.Id)).ToListAsync();
        if (emailAccounts.Count < 1)
            throw new BadRequestException("无可删除数据!");

        return await LogicDelete<EmailAccount>(x => ids.Contains(x.Id)) > 0;
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
            await SugarRepository.QueryPageListAsync(queryOptions));
    }

    public async Task<List<ExportBase>> DownloadAsync(EmailAccountQueryCriteria emailAccountQueryCriteria)
    {
        var emailAccounts = await TableWhere(emailAccountQueryCriteria.ApplyQueryConditionalModel()).ToListAsync();
        List<ExportBase> emailAccountExports = new List<ExportBase>();
        emailAccountExports.AddRange(emailAccounts.Select(x => new EmailAccountExport()
        {
            Email = x.Email,
            DisplayName = x.DisplayName,
            Host = x.Host,
            Port = x.Port,
            Username = x.Username,
            EnableSsl = x.EnableSsl ? BoolState.True : BoolState.False,
            UseDefaultCredentials = x.UseDefaultCredentials ? BoolState.True : BoolState.False,
            CreateTime = x.CreateTime
        }));
        return emailAccountExports;
    }

    #endregion
}
