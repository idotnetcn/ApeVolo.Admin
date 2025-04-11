using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ape.Volo.Business.Base;
using Ape.Volo.Common;
using Ape.Volo.Common.Attributes;
using Ape.Volo.Common.Exception;
using Ape.Volo.Common.Extensions;
using Ape.Volo.Common.Global;
using Ape.Volo.Common.Model;
using Ape.Volo.Entity.System;
using Ape.Volo.IBusiness.Dto.System;
using Ape.Volo.IBusiness.ExportModel.System;
using Ape.Volo.IBusiness.Interface.System;
using Ape.Volo.IBusiness.QueryModel;

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

    public QuartzNetService(IQuartzNetLogService quartzNetLogService)
    {
        _quartzNetLogService = quartzNetLogService;
    }

    #endregion

    #region 基础方法

    public async Task<List<QuartzNet>> QueryAllAsync()
    {
        return await Table.ToListAsync();
    }


    public async Task<QuartzNet> CreateAsync(CreateUpdateQuartzNetDto createUpdateQuartzNetDto)
    {
        if (await TableWhere(q =>
                q.AssemblyName == createUpdateQuartzNetDto.AssemblyName &&
                q.ClassName == createUpdateQuartzNetDto.ClassName).AnyAsync())
        {
            throw new BadRequestException(
                $"作业执行目录=>{createUpdateQuartzNetDto.AssemblyName + "_" + createUpdateQuartzNetDto.ClassName}=>已存在!");
        }

        var quartzNet = App.Mapper.MapTo<QuartzNet>(createUpdateQuartzNetDto);
        return await SugarClient.Insertable(quartzNet).ExecuteReturnEntityAsync();
    }

    public async Task<OperateResult> UpdateAsync(CreateUpdateQuartzNetDto createUpdateQuartzNetDto)
    {
        var oldQuartzNet =
            await TableWhere(x => x.Id == createUpdateQuartzNetDto.Id).FirstAsync();
        if (oldQuartzNet.IsNull())
        {
            return OperateResult.Error("数据不存在！");
        }

        if ((oldQuartzNet.AssemblyName != createUpdateQuartzNetDto.AssemblyName ||
             oldQuartzNet.ClassName != createUpdateQuartzNetDto.ClassName) && await TableWhere(q =>
                q.AssemblyName == createUpdateQuartzNetDto.AssemblyName &&
                q.ClassName == createUpdateQuartzNetDto.ClassName).AnyAsync())
        {
            return OperateResult.Error(
                $"作业执行目录=>{createUpdateQuartzNetDto.AssemblyName + "_" + createUpdateQuartzNetDto.ClassName}=>已存在!");
        }

        var quartzNet = App.Mapper.MapTo<QuartzNet>(createUpdateQuartzNetDto);
        var result = await UpdateAsync(quartzNet);
        return OperateResult.Result(result);
    }


    [UseTran]
    public async Task<OperateResult> UpdateJobInfoAsync(QuartzNet quartzNet, QuartzNetLog quartzNetLog)
    {
        await UpdateAsync(quartzNet);
        await _quartzNetLogService.CreateAsync(quartzNetLog);
        return OperateResult.Success();
    }

    public async Task<OperateResult> DeleteAsync(List<QuartzNet> quartzNets)
    {
        var ids = quartzNets.Select(x => x.Id).ToList();
        var result = await LogicDelete<QuartzNet>(x => ids.Contains(x.Id));
        return OperateResult.Result(result);
    }

    public async Task<List<QuartzNetDto>> QueryAsync(QuartzNetQueryCriteria quartzNetQueryCriteria,
        Pagination pagination)
    {
        var queryOptions = new QueryOptions<QuartzNet>
        {
            Pagination = pagination,
            ConditionalModels = quartzNetQueryCriteria.ApplyQueryConditionalModel()
        };
        return App.Mapper.MapTo<List<QuartzNetDto>>(
            await TablePageAsync(queryOptions));
    }

    public async Task<List<ExportBase>> DownloadAsync(QuartzNetQueryCriteria quartzNetQueryCriteria)
    {
        var quartzNets = await TableWhere(quartzNetQueryCriteria.ApplyQueryConditionalModel()).ToListAsync();
        List<ExportBase> quartzExports = new List<ExportBase>();
        quartzExports.AddRange(quartzNets.Select(x => new QuartzNetExport()
        {
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
            IsEnable = x.IsEnable ? BoolState.True : BoolState.False,
            RunParams = x.RunParams,
            TriggerStatus = x.TriggerStatus,
            TriggerTypeStr = x.TriggerType.GetDisplayName(),
            CreateTime = x.CreateTime
        }));
        return quartzExports;
    }

    #endregion
}
