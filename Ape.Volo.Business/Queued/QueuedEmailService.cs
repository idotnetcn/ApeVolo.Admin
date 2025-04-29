using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ape.Volo.Business.Base;
using Ape.Volo.Common;
using Ape.Volo.Common.Enums;
using Ape.Volo.Common.Extensions;
using Ape.Volo.Common.Global;
using Ape.Volo.Common.Helper;
using Ape.Volo.Common.Model;
using Ape.Volo.Entity.Queued;
using Ape.Volo.IBusiness.Dto.Queued;
using Ape.Volo.IBusiness.Interface.Message.Email;
using Ape.Volo.IBusiness.Interface.Queued;
using Ape.Volo.IBusiness.QueryModel;
using Microsoft.Extensions.Logging;

namespace Ape.Volo.Business.Queued;

/// <summary>
/// 邮件队列接口实现
/// </summary>
public class QueuedEmailService : BaseServices<QueuedEmail>, IQueuedEmailService
{
    #region 字段

    private readonly IEmailMessageTemplateService _emailMessageTemplateService;
    private readonly IEmailAccountService _emailAccountService;
    private readonly IEmailSender _emailSender;
    private readonly ILogger<QueuedEmailService> _logger;

    #endregion

    #region 构造函数

    public QueuedEmailService(IEmailMessageTemplateService emailMessageTemplateService,
        IEmailAccountService emailAccountService, IEmailSender emailSender, ILogger<QueuedEmailService> logger)
    {
        _emailMessageTemplateService = emailMessageTemplateService;
        _emailAccountService = emailAccountService;
        _emailSender = emailSender;
        _logger = logger;
    }

    #endregion

    #region 基础方法

    /// <summary>
    /// 新增
    /// </summary>
    /// <param name="createUpdateQueuedEmailDto"></param>
    /// <returns></returns>
    public async Task<OperateResult> CreateAsync(CreateUpdateQueuedEmailDto createUpdateQueuedEmailDto)
    {
        var emailAccount = await _emailAccountService.TableWhere(x => x.Id == createUpdateQueuedEmailDto.EmailAccountId)
            .SingleAsync();
        if (emailAccount.IsNull())
        {
            return OperateResult.Error(DataErrorHelper.NotExist(createUpdateQueuedEmailDto,
                LanguageKeyConstants.EmailAccount,
                nameof(createUpdateQueuedEmailDto.Id)));
        }

        createUpdateQueuedEmailDto.From = emailAccount.Email;
        createUpdateQueuedEmailDto.FromName = emailAccount.DisplayName;
        var queuedEmail = App.Mapper.MapTo<QueuedEmail>(createUpdateQueuedEmailDto);
        var result = await AddAsync(queuedEmail);
        return OperateResult.Result(result);
    }

    /// <summary>
    /// 更新
    /// </summary>
    /// <param name="queuedEmailDto"></param>
    /// <returns></returns>
    public async Task<OperateResult> UpdateTriesAsync(QueuedEmailDto queuedEmailDto)
    {
        var queuedEmail = App.Mapper.MapTo<QueuedEmail>(queuedEmailDto);
        var result = await UpdateAsync(queuedEmail);
        return OperateResult.Result(result);
    }

    /// <summary>
    /// 更新
    /// </summary>
    /// <param name="createUpdateQueuedEmailDto"></param>
    /// <returns></returns>
    public async Task<OperateResult> UpdateAsync(CreateUpdateQueuedEmailDto createUpdateQueuedEmailDto)
    {
        if (!await TableWhere(x => x.Id == createUpdateQueuedEmailDto.Id).AnyAsync())
        {
            return OperateResult.Error(DataErrorHelper.NotExist(createUpdateQueuedEmailDto,
                LanguageKeyConstants.QueuedEmail,
                nameof(createUpdateQueuedEmailDto.Id)));
        }

        var emailAccount = await _emailAccountService.TableWhere(x => x.Id == createUpdateQueuedEmailDto.EmailAccountId)
            .SingleAsync();
        if (emailAccount.IsNull())
        {
            return OperateResult.Error(DataErrorHelper.NotExist(createUpdateQueuedEmailDto,
                LanguageKeyConstants.EmailAccount,
                nameof(createUpdateQueuedEmailDto.EmailAccountId)));
        }

        createUpdateQueuedEmailDto.From = emailAccount.Email;
        createUpdateQueuedEmailDto.FromName = emailAccount.DisplayName;
        var queuedEmail = App.Mapper.MapTo<QueuedEmail>(createUpdateQueuedEmailDto);
        var result = await UpdateAsync(queuedEmail);
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

        var result = await LogicDelete<QueuedEmail>(x => ids.Contains(x.Id));
        return OperateResult.Result(result);
    }

    /// <summary>
    /// 查询
    /// </summary>
    /// <param name="queuedEmailQueryCriteria"></param>
    /// <param name="pagination"></param>
    /// <returns></returns>
    public async Task<List<QueuedEmailDto>> QueryAsync(QueuedEmailQueryCriteria queuedEmailQueryCriteria,
        Pagination pagination)
    {
        var queryOptions = new QueryOptions<QueuedEmail>
        {
            Pagination = pagination,
            ConditionalModels = queuedEmailQueryCriteria.ApplyQueryConditionalModel(),
        };
        return App.Mapper.MapTo<List<QueuedEmailDto>>(
            await TablePageAsync(queryOptions));
    }

    #endregion

    #region 扩展方法

    /// <summary>
    /// 变更邮箱验证码
    /// </summary>
    /// <param name="emailAddress"></param>
    /// <param name="messageTemplateName"></param>
    /// <returns></returns>
    public async Task<OperateResult> ResetEmail(string emailAddress, string messageTemplateName)
    {
        var emailMessageTemplate =
            await _emailMessageTemplateService.TableWhere(x => x.Name == messageTemplateName).FirstAsync();
        if (emailMessageTemplate.IsNull())
        {
            return OperateResult.Error(DataErrorHelper.NotExist());
        }

        var emailAccount = await _emailAccountService.TableWhere(x => x.Id == emailMessageTemplate.EmailAccountId)
            .SingleAsync();
        if (emailAccount.IsNull())
        {
            return OperateResult.Error(DataErrorHelper.NotExist());
        }

        //生成6位随机码
        var captcha = SixLaborsImageHelper.BuilEmailCaptcha(6);

        QueuedEmail queuedEmail = new QueuedEmail();
        queuedEmail.From = emailAccount.Email;
        queuedEmail.FromName = emailAccount.DisplayName;
        queuedEmail.To = emailAddress;
        queuedEmail.Priority = QueuedEmailPriority.High;
        queuedEmail.Bcc = emailMessageTemplate.BccEmailAddresses;
        queuedEmail.Subject = emailMessageTemplate.Subject;
        queuedEmail.Body = emailMessageTemplate.Body.Replace("%captcha%", captcha);
        queuedEmail.SentTries = 1;
        queuedEmail.EmailAccountId = emailAccount.Id;

        await App.Cache.RemoveAsync(GlobalConstants.CachePrefix.EmailCaptcha +
                                    queuedEmail.To.ToMd5String());
        var isTrue = await App.Cache.SetAsync(
            GlobalConstants.CachePrefix.EmailCaptcha + queuedEmail.To.ToMd5String(), captcha,
            TimeSpan.FromMinutes(5), null);

        if (isTrue)
        {
            var bcc = string.IsNullOrWhiteSpace(queuedEmail.Bcc)
                ? null
                : queuedEmail.Bcc.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            var cc = string.IsNullOrWhiteSpace(queuedEmail.Cc)
                ? null
                : queuedEmail.Cc.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            try
            {
                isTrue = await _emailSender.SendEmailAsync(
                    await _emailAccountService.TableWhere(x => x.Id == queuedEmail.EmailAccountId).FirstAsync(),
                    queuedEmail.Subject,
                    queuedEmail.Body,
                    queuedEmail.From,
                    queuedEmail.FromName,
                    queuedEmail.To,
                    queuedEmail.ToName,
                    queuedEmail.ReplyTo,
                    queuedEmail.ReplyToName,
                    bcc,
                    cc);
                queuedEmail.IsSend = isTrue;
                if (isTrue)
                {
                    queuedEmail.SendTime = DateTime.Now;
                }
                // 如果开启redis并且开启消息队列功能 可以使用下面方式
                // await App.Cache.GetDatabase()
                //     .ListLeftPushAsync(MqTopicNameKey.MailboxQueue, queuedEmail.Id.ToString());
            }
            catch (Exception exc)
            {
                _logger.LogError($"Error sending e-mail. {exc.Message}");
                isTrue = false;
            }
            finally
            {
                try
                {
                    await AddAsync(queuedEmail);
                }
                catch
                {
                    // ignored
                }
            }
        }

        return OperateResult.Success();
    }

    #endregion
}
