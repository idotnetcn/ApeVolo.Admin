using System.Collections.Generic;
using System.Threading.Tasks;
using Ape.Volo.Business.Base;
using Ape.Volo.Common;
using Ape.Volo.Common.Extensions;
using Ape.Volo.Common.Model;
using Ape.Volo.Entity.Monitor;
using Ape.Volo.IBusiness.Dto.Monitor;
using Ape.Volo.IBusiness.Interface.Monitor;
using Ape.Volo.IBusiness.QueryModel;

namespace Ape.Volo.Business.Monitor;

/// <summary>
/// 系统日志服务
/// </summary>
public class ExceptionLogService : BaseServices<ExceptionLog>, IExceptionLogService
{
    #region 构造函数

    public ExceptionLogService()
    {
    }

    #endregion

    #region 基础方法

    public async Task<bool> CreateAsync(ExceptionLog exceptionLog)
    {
        //return await AddEntityAsync(exceptionLog);
        return await SugarRepository.SugarClient.Insertable(exceptionLog).SplitTable().ExecuteCommandAsync() > 0;
    }

    public async Task<List<ExceptionLogDto>> QueryAsync(LogQueryCriteria logQueryCriteria, Pagination pagination)
    {
        var queryOptions = new QueryOptions<ExceptionLog>
        {
            Pagination = pagination,
            ConditionalModels = logQueryCriteria.ApplyQueryConditionalModel(),
            IsSplitTable = true
        };
        var logs = await SugarRepository.QueryPageListAsync(queryOptions);
        return App.Mapper.MapTo<List<ExceptionLogDto>>(logs);
    }

    #endregion
}
