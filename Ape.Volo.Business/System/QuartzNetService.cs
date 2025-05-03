using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ape.Volo.Common.Attributes;
using Ape.Volo.Common.Exception;
using Ape.Volo.Common.Extensions;
using Ape.Volo.Common.Global;
using Ape.Volo.Common.Model;
using Ape.Volo.Core;
using Ape.Volo.Core.Utils;
using Ape.Volo.Entity.Core.System.QuartzNet;
using Ape.Volo.IBusiness.System;
using Ape.Volo.SharedModel.Dto.Core.System;
using Ape.Volo.SharedModel.Queries.Common;
using Ape.Volo.SharedModel.Queries.System;
using Ape.Volo.ViewModel.Core.System.QuartzNet;
using Ape.Volo.ViewModel.Report.System;

namespace Ape.Volo.Business.System;

/// <summary>
/// QuartzNet作业服务
/// </summary>
public class QuartzNetService : BaseServices<QuartzNet>, IQuartzNetService
{
    #region 字段

    private readonly IQuartzNetLogService _quartzNetLogService;

    #endregion

    #region 构造函数

    /// <summary>
    /// 
    /// </summary>
    /// <param name="quartzNetLogService"></param>
    public QuartzNetService(IQuartzNetLogService quartzNetLogService)
    {
        _quartzNetLogService = quartzNetLogService;
    }

    #endregion

    #region 基础方法

    /// <summary>
    /// 查询全部
    /// </summary>
    /// <returns></returns>
    public async Task<List<QuartzNet>> QueryAllAsync()
    {
        return await Table.ToListAsync();
    }

    /// <summary>
    /// 创建
    /// </summary>
    /// <param name="createUpdateQuartzNetDto"></param>
    /// <returns></returns>
    /// <exception cref="BadRequestException"></exception>
    public async Task<QuartzNet> CreateAsync(CreateUpdateQuartzNetDto createUpdateQuartzNetDto)
    {
        if (await TableWhere(q =>
                q.AssemblyName == createUpdateQuartzNetDto.AssemblyName &&
                q.ClassName == createUpdateQuartzNetDto.ClassName).AnyAsync())
        {
            throw new BadRequestException(
                ValidationError.IsExist(createUpdateQuartzNetDto, nameof(createUpdateQuartzNetDto.ClassName)));
        }

        var quartzNet = App.Mapper.MapTo<QuartzNet>(createUpdateQuartzNetDto);
        return await SugarClient.Insertable(quartzNet).ExecuteReturnEntityAsync();
    }

    /// <summary>
    /// 更新
    /// </summary>
    /// <param name="createUpdateQuartzNetDto"></param>
    /// <returns></returns>
    public async Task<OperateResult> UpdateAsync(CreateUpdateQuartzNetDto createUpdateQuartzNetDto)
    {
        var oldQuartzNet =
            await TableWhere(x => x.Id == createUpdateQuartzNetDto.Id).FirstAsync();
        if (oldQuartzNet.IsNull())
        {
            return OperateResult.Error(ValidationError.NotExist(createUpdateQuartzNetDto,
                LanguageKeyConstants.QuartzNet,
                nameof(createUpdateQuartzNetDto.Id)));
        }

        if ((oldQuartzNet.AssemblyName != createUpdateQuartzNetDto.AssemblyName ||
             oldQuartzNet.ClassName != createUpdateQuartzNetDto.ClassName) && await TableWhere(q =>
                q.AssemblyName == createUpdateQuartzNetDto.AssemblyName &&
                q.ClassName == createUpdateQuartzNetDto.ClassName).AnyAsync())
        {
            return OperateResult.Error(
                ValidationError.IsExist(createUpdateQuartzNetDto, nameof(createUpdateQuartzNetDto.ClassName)));
        }

        var quartzNet = App.Mapper.MapTo<QuartzNet>(createUpdateQuartzNetDto);
        var result = await UpdateAsync(quartzNet);
        return OperateResult.Result(result);
    }

    /// <summary>
    /// 更新作业
    /// </summary>
    /// <param name="quartzNet"></param>
    /// <param name="quartzNetLog"></param>
    /// <returns></returns>
    [UseTran]
    public async Task<OperateResult> UpdateJobInfoAsync(QuartzNet quartzNet, QuartzNetLog quartzNetLog)
    {
        await UpdateAsync(quartzNet);
        await _quartzNetLogService.CreateAsync(quartzNetLog);
        return OperateResult.Success();
    }

    /// <summary>
    /// 删除
    /// </summary>
    /// <param name="quartzNets"></param>
    /// <returns></returns>
    public async Task<OperateResult> DeleteAsync(List<QuartzNet> quartzNets)
    {
        var ids = quartzNets.Select(x => x.Id).ToList();
        var result = await LogicDelete<QuartzNet>(x => ids.Contains(x.Id));
        return OperateResult.Result(result);
    }

    /// <summary>
    /// 查询
    /// </summary>
    /// <param name="quartzNetQueryCriteria"></param>
    /// <param name="pagination"></param>
    /// <returns></returns>
    public async Task<List<QuartzNetVo>> QueryAsync(QuartzNetQueryCriteria quartzNetQueryCriteria,
        Pagination pagination)
    {
        var queryOptions = new QueryOptions<QuartzNet>
        {
            Pagination = pagination,
            ConditionalModels = quartzNetQueryCriteria.ApplyQueryConditionalModel()
        };
        return App.Mapper.MapTo<List<QuartzNetVo>>(
            await TablePageAsync(queryOptions));
    }

    /// <summary>
    /// 下载
    /// </summary>
    /// <param name="quartzNetQueryCriteria"></param>
    /// <returns></returns>
    public async Task<List<ExportBase>> DownloadAsync(QuartzNetQueryCriteria quartzNetQueryCriteria)
    {
        var quartzNets = await TableWhere(quartzNetQueryCriteria.ApplyQueryConditionalModel()).ToListAsync();
        List<ExportBase> quartzExports = new List<ExportBase>();
        quartzExports.AddRange(quartzNets.Select(x => new QuartzNetExport
        {
            Id = x.Id,
            TaskName = x.TaskName,
            TaskGroup = x.TaskGroup,
            Cron = x.Cron,
            AssemblyName = x.AssemblyName,
            ClassName = x.ClassName,
            Description = x.Description,
            Principal = x.Principal,
            AlertEmail = x.AlertEmail,
            PauseAfterFailure = x.PauseAfterFailure,
            RunTimes = x.RunTimes,
            StartTime = x.StartTime,
            EndTime = x.EndTime,
            TriggerType = x.TriggerType,
            IntervalSecond = x.IntervalSecond,
            CycleRunTimes = x.CycleRunTimes,
            IsEnable = x.IsEnable,
            RunParams = x.RunParams,
            TriggerStatus = x.TriggerStatus,
            CreateTime = x.CreateTime
        }));
        return quartzExports;
    }

    #endregion
}
