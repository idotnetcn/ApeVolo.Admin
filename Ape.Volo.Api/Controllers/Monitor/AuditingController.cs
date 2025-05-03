using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Ape.Volo.Api.Controllers.Base;
using Ape.Volo.Common.Attributes;
using Ape.Volo.Common.Model;
using Ape.Volo.IBusiness.Monitor;
using Ape.Volo.SharedModel.Queries.Common;
using Ape.Volo.SharedModel.Queries.System;
using Ape.Volo.ViewModel.Core.Monitor;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Ape.Volo.Api.Controllers.Monitor;

/// <summary>
/// 审计日志管理
/// </summary>
[Area("Area.AuditLogManagement")]
[Route("/api/auditing", Order = 13)]
public class AuditingController : BaseApiController
{
    #region 字段

    private readonly IAuditLogService _auditInfoService;

    #endregion

    #region 构造函数

    public AuditingController(IAuditLogService auditInfoService)
    {
        _auditInfoService = auditInfoService;
    }

    #endregion

    #region 内部接口

    /// <summary>
    /// 审计列表
    /// </summary>
    /// <param name="logQueryCriteria">查询对象</param>
    /// <param name="pagination">分页对象</param>
    /// <returns></returns>
    [HttpGet]
    [Route("query")]
    [Description("Sys.Query")]
    [NotAudit]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ActionResultVm<List<AuditLogVo>>))]
    public async Task<ActionResult> Query(LogQueryCriteria logQueryCriteria,
        Pagination pagination)
    {
        var auditInfos = await _auditInfoService.QueryAsync(logQueryCriteria, pagination);

        return JsonContent(auditInfos, pagination);
    }


    /// <summary>
    /// 当前用户行为
    /// </summary>
    /// <param name="pagination"></param>
    /// <returns></returns>
    [HttpGet]
    [Route("current")]
    [Description("Action.UserConduct")]
    [NotAudit]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ActionResultVm<List<AuditLogVo>>))]
    public async Task<ActionResult> FindListByCurrent(Pagination pagination)
    {
        var auditInfos = await _auditInfoService.QueryByCurrentAsync(pagination);

        return JsonContent(auditInfos, pagination);
    }

    #endregion
}
