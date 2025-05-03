using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ape.Volo.Common.Extensions;
using Ape.Volo.Common.Model;
using Ape.Volo.Core;
using Ape.Volo.Entity.Core.System.QuartzNet;
using Ape.Volo.IBusiness.System;
using Ape.Volo.SharedModel.Queries.Common;
using Ape.Volo.SharedModel.Queries.System;
using Ape.Volo.ViewModel.Core.System.QuartzNet;
using Ape.Volo.ViewModel.Report.System;

namespace Ape.Volo.Business.System;

/// <summary>
/// QuartzNet作业日志服务
/// </summary>
public class QuartzNetLogService : BaseServices<QuartzNetLog>, IQuartzNetLogService
{
    #region 基础方法

    /// <summary>
    /// 创建
    /// </summary>
    /// <param name="quartzNetLog"></param>
    /// <returns></returns>
    public async Task<OperateResult> CreateAsync(QuartzNetLog quartzNetLog)
    {
        var result = await AddAsync(quartzNetLog);
        return OperateResult.Result(result);
    }

    /// <summary>
    /// 查询
    /// </summary>
    /// <param name="quartzNetLogQueryCriteria"></param>
    /// <param name="pagination"></param>
    /// <returns></returns>
    public async Task<List<QuartzNetLogVo>> QueryAsync(QuartzNetLogQueryCriteria quartzNetLogQueryCriteria,
        Pagination pagination)
    {
        var queryOptions = new QueryOptions<QuartzNetLog>
        {
            Pagination = pagination,
            ConditionalModels = quartzNetLogQueryCriteria.ApplyQueryConditionalModel()
        };
        return App.Mapper.MapTo<List<QuartzNetLogVo>>(
            await TablePageAsync(queryOptions));
    }

    /// <summary>
    /// 下载
    /// </summary>
    /// <param name="quartzNetLogQueryCriteria"></param>
    /// <returns></returns>
    public async Task<List<ExportBase>> DownloadAsync(QuartzNetLogQueryCriteria quartzNetLogQueryCriteria)
    {
        var quartzNetLogs = await TableWhere(quartzNetLogQueryCriteria.ApplyQueryConditionalModel()).ToListAsync();
        List<ExportBase> quartzNetLogExports = new List<ExportBase>();
        quartzNetLogExports.AddRange(quartzNetLogs.Select(x => new QuartzNetLogExport
        {
            Id = x.Id,
            TaskId = x.TaskId,
            TaskName = x.TaskName,
            TaskGroup = x.TaskGroup,
            AssemblyName = x.AssemblyName,
            ClassName = x.ClassName,
            Cron = x.Cron,
            ExceptionDetail = x.ExceptionDetail,
            ExecutionDuration = x.ExecutionDuration,
            RunParams = x.RunParams,
            IsSuccess = x.IsSuccess,
            CreateTime = x.CreateTime
        }));
        return quartzNetLogExports;
    }

    #endregion
}
