using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Ape.Volo.Common.Extensions;
using Ape.Volo.Common.Global;
using Ape.Volo.Common.Model;
using Ape.Volo.Core;
using Ape.Volo.Core.Utils;
using Ape.Volo.Entity.Core.Permission;
using Ape.Volo.IBusiness.Permission;
using Ape.Volo.SharedModel.Dto.Core.Permission;
using Ape.Volo.SharedModel.Queries.Common;
using Ape.Volo.SharedModel.Queries.Permission;
using Ape.Volo.ViewModel.Core.Permission.Job;
using Ape.Volo.ViewModel.Report.Permission;
using SqlSugar;

namespace Ape.Volo.Business.Permission;

/// <summary>
/// 岗位服务
/// </summary>
public class JobService : BaseServices<Job>, IJobService
{
    #region 基础方法

    /// <summary>
    /// 创建
    /// </summary>
    /// <param name="createUpdateJobDto"></param>
    /// <returns></returns>
    public async Task<OperateResult> CreateAsync(CreateUpdateJobDto createUpdateJobDto)
    {
        if (await TableWhere(j => j.Name == createUpdateJobDto.Name).AnyAsync())
        {
            return OperateResult.Error(ValidationError.IsExist(createUpdateJobDto,
                nameof(createUpdateJobDto.Name)));
        }

        var job = App.Mapper.MapTo<Job>(createUpdateJobDto);
        var result = await AddAsync(job);
        return OperateResult.Result(result);
    }

    /// <summary>
    /// 更新
    /// </summary>
    /// <param name="createUpdateJobDto"></param>
    /// <returns></returns>
    public async Task<OperateResult> UpdateAsync(CreateUpdateJobDto createUpdateJobDto)
    {
        var oldJob =
            await TableWhere(x => x.Id == createUpdateJobDto.Id).FirstAsync();
        if (oldJob.IsNull())
        {
            return OperateResult.Error(ValidationError.NotExist(createUpdateJobDto, LanguageKeyConstants.Job,
                nameof(createUpdateJobDto.Id)));
        }

        if (oldJob.Name != createUpdateJobDto.Name &&
            await TableWhere(j => j.Name == createUpdateJobDto.Name).AnyAsync())
        {
            return OperateResult.Error(ValidationError.IsExist(createUpdateJobDto,
                nameof(createUpdateJobDto.Name)));
        }

        var job = App.Mapper.MapTo<Job>(createUpdateJobDto);
        var result = await UpdateAsync(job);
        return OperateResult.Result(result);
    }

    /// <summary>
    /// 删除
    /// </summary>
    /// <param name="ids"></param>
    /// <returns></returns>
    public async Task<OperateResult> DeleteAsync(HashSet<long> ids)
    {
        var jobs = await TableWhere(x => ids.Contains(x.Id)).Includes(x => x.Users).ToListAsync();
        if (jobs.Count < 1)
        {
            return OperateResult.Error(ValidationError.NotExist());
        }

        if (jobs.Any(job => job.Users != null && job.Users.Count != 0))
        {
            return OperateResult.Error(ValidationError.DataAssociationExists());
        }

        var result = await LogicDelete<Job>(x => ids.Contains(x.Id));
        return OperateResult.Result(result);
    }

    /// <summary>
    /// 查询
    /// </summary>
    /// <param name="jobQueryCriteria"></param>
    /// <param name="pagination"></param>
    /// <returns></returns>
    public async Task<List<JobVo>> QueryAsync(JobQueryCriteria jobQueryCriteria, Pagination pagination)
    {
        var queryOptions = new QueryOptions<Job>
        {
            Pagination = pagination,
            ConditionalModels = jobQueryCriteria.ApplyQueryConditionalModel(),
        };


        return App.Mapper.MapTo<List<JobVo>>(await TablePageAsync(queryOptions));
    }

    /// <summary>
    /// 下载
    /// </summary>
    /// <param name="jobQueryCriteria"></param>
    /// <returns></returns>
    public async Task<List<ExportBase>> DownloadAsync(JobQueryCriteria jobQueryCriteria)
    {
        var conditionalModels = jobQueryCriteria.ApplyQueryConditionalModel();
        var jobs = await TableWhere(conditionalModels).ToListAsync();
        List<ExportBase> roleExports = new List<ExportBase>();
        roleExports.AddRange(jobs.Select(x => new JobExport
        {
            Id = x.Id,
            Name = x.Name,
            Sort = x.Sort,
            Enabled = x.Enabled,
            CreateTime = x.CreateTime
        }));
        return roleExports;
    }

    #endregion

    #region 扩展方法

    /// <summary>
    /// 查询全部
    /// </summary>
    /// <returns></returns>
    public async Task<List<JobVo>> QueryAllAsync()
    {
        Expression<Func<Job, bool>> whereExpression = x => x.Enabled;


        return App.Mapper.MapTo<List<JobVo>>(await TableWhere(whereExpression, null, x => x.Sort, OrderByType.Asc)
            .ToListAsync());
    }

    #endregion
}
