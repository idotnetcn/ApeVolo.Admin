using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ape.Volo.Business.Base;
using Ape.Volo.Common;
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
/// QuartzNet作业日志服务
/// </summary>
public class QuartzNetLogService : BaseServices<QuartzNetLog>, IQuartzNetLogService
{
    #region 构造函数

    public QuartzNetLogService()
    {
    }

    #endregion

    #region 基础方法

    public async Task<OperateResult> CreateAsync(QuartzNetLog quartzNetLog)
    {
        var result = await AddAsync(quartzNetLog);
        return OperateResult.Result(result);
    }

    public async Task<List<QuartzNetLogDto>> QueryAsync(QuartzNetLogQueryCriteria quartzNetLogQueryCriteria,
        Pagination pagination)
    {
        var queryOptions = new QueryOptions<QuartzNetLog>
        {
            Pagination = pagination,
            ConditionalModels = quartzNetLogQueryCriteria.ApplyQueryConditionalModel()
        };
        return App.Mapper.MapTo<List<QuartzNetLogDto>>(
            await TablePageAsync(queryOptions));
    }

    public async Task<List<ExportBase>> DownloadAsync(QuartzNetLogQueryCriteria quartzNetLogQueryCriteria)
    {
        var quartzNetLogs = await TableWhere(quartzNetLogQueryCriteria.ApplyQueryConditionalModel()).ToListAsync();
        List<ExportBase> quartzNetLogExports = new List<ExportBase>();
        quartzNetLogExports.AddRange(quartzNetLogs.Select(x => new QuartzNetLogExport()
        {
            TaskId = x.TaskId,
            TaskName = x.TaskName,
            TaskGroup = x.TaskGroup,
            AssemblyName = x.AssemblyName,
            ClassName = x.ClassName,
            Cron = x.Cron,
            ExceptionDetail = x.ExceptionDetail,
            ExecutionDuration = x.ExecutionDuration,
            RunParams = x.RunParams,
            IsSuccess = x.IsSuccess ? BoolState.True : BoolState.False,
            CreateTime = x.CreateTime
        }));
        return quartzNetLogExports;
    }

    #endregion
}
