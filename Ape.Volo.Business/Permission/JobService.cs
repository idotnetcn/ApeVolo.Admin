using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Ape.Volo.Business.Base;
using Ape.Volo.Common;
using Ape.Volo.Common.Extensions;
using Ape.Volo.Common.Global;
using Ape.Volo.Common.Helper;
using Ape.Volo.Common.Model;
using Ape.Volo.Entity.Permission;
using Ape.Volo.IBusiness.Dto.Permission;
using Ape.Volo.IBusiness.ExportModel.Permission;
using Ape.Volo.IBusiness.Interface.Permission;
using Ape.Volo.IBusiness.QueryModel;
using SqlSugar;

namespace Ape.Volo.Business.Permission;

public class JobService : BaseServices<Job>, IJobService
{
    #region 基础方法

    public async Task<OperateResult> CreateAsync(CreateUpdateJobDto createUpdateJobDto)
    {
        if (await TableWhere(j => j.Name == createUpdateJobDto.Name).AnyAsync())
        {
            return OperateResult.Error(DataErrorHelper.IsExist(createUpdateJobDto,
                nameof(createUpdateJobDto.Name)));
        }

        var job = App.Mapper.MapTo<Job>(createUpdateJobDto);
        var result = await AddAsync(job);
        return OperateResult.Result(result);
    }

    public async Task<OperateResult> UpdateAsync(CreateUpdateJobDto createUpdateJobDto)
    {
        var oldJob =
            await TableWhere(x => x.Id == createUpdateJobDto.Id).FirstAsync();
        if (oldJob.IsNull())
        {
            return OperateResult.Error(DataErrorHelper.NotExist(createUpdateJobDto, LanguageKeyConstants.Job,
                nameof(createUpdateJobDto.Id)));
        }

        if (oldJob.Name != createUpdateJobDto.Name &&
            await TableWhere(j => j.Name == createUpdateJobDto.Name).AnyAsync())
        {
            return OperateResult.Error(DataErrorHelper.IsExist(createUpdateJobDto,
                nameof(createUpdateJobDto.Name)));
        }

        var job = App.Mapper.MapTo<Job>(createUpdateJobDto);
        var result = await UpdateAsync(job);
        return OperateResult.Result(result);
    }

    public async Task<OperateResult> DeleteAsync(HashSet<long> ids)
    {
        var jobs = await TableWhere(x => ids.Contains(x.Id)).Includes(x => x.Users).ToListAsync();
        if (jobs.Count < 1)
        {
            return OperateResult.Error(DataErrorHelper.NotExist());
        }

        if (jobs.Any(job => job.Users != null && job.Users.Count != 0))
        {
            return OperateResult.Error(DataErrorHelper.DataAssociationExists());
        }

        var result = await LogicDelete<Job>(x => ids.Contains(x.Id));
        return OperateResult.Result(result);
    }


    public async Task<List<JobDto>> QueryAsync(JobQueryCriteria jobQueryCriteria, Pagination pagination)
    {
        var queryOptions = new QueryOptions<Job>
        {
            Pagination = pagination,
            ConditionalModels = jobQueryCriteria.ApplyQueryConditionalModel(),
        };


        return App.Mapper.MapTo<List<JobDto>>(await TablePageAsync(queryOptions));
    }

    public async Task<List<ExportBase>> DownloadAsync(JobQueryCriteria jobQueryCriteria)
    {
        var conditionalModels = jobQueryCriteria.ApplyQueryConditionalModel();
        var jbos = await TableWhere(conditionalModels).ToListAsync();
        List<ExportBase> roleExports = new List<ExportBase>();
        roleExports.AddRange(jbos.Select(x => new JobExport
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

    public async Task<List<JobDto>> QueryAllAsync()
    {
        Expression<Func<Job, bool>> whereExpression = x => x.Enabled;


        return App.Mapper.MapTo<List<JobDto>>(await TableWhere(whereExpression, null, x => x.Sort, OrderByType.Asc)
            .ToListAsync());
    }

    #endregion
}
