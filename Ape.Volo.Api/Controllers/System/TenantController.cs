using System.ComponentModel;
using System.Threading.Tasks;
using Ape.Volo.Api.Controllers.Base;
using Ape.Volo.Common.Extensions;
using Ape.Volo.Common.Helper;
using Ape.Volo.Common.Model;
using Ape.Volo.IBusiness.Dto.System;
using Ape.Volo.IBusiness.Interface.System;
using Ape.Volo.IBusiness.QueryModel;
using Ape.Volo.IBusiness.RequestModel;
using Microsoft.AspNetCore.Mvc;

namespace Ape.Volo.Api.Controllers.System;

/// <summary>
/// 租户管理
/// </summary>
[Area("Area.TenantManagement")]
[Route("/api/tenant", Order = 19)]
public class TenantController : BaseApiController
{
    #region 字段

    private readonly ITenantService _tenantService;

    #endregion

    #region 构造函数

    public TenantController(ITenantService tenantService)
    {
        _tenantService = tenantService;
    }

    #endregion

    #region 内部接口

    /// <summary>
    /// 新增租户
    /// </summary>
    /// <param name="createUpdateTenantDto"></param>
    /// <returns></returns>
    [HttpPost]
    [Route("create")]
    [Description("Sys.Create")]
    public async Task<ActionResult> Create(
        [FromBody] CreateUpdateTenantDto createUpdateTenantDto)
    {
        if (!ModelState.IsValid)
        {
            var actionError = ModelState.GetErrors();
            return Error(actionError);
        }

        var result = await _tenantService.CreateAsync(createUpdateTenantDto);
        return Ok(result);
    }

    /// <summary>
    /// 更新租户
    /// </summary>
    /// <param name="createUpdateTenantDto"></param>
    /// <returns></returns>
    [HttpPut]
    [Route("edit")]
    [Description("Sys.Edit")]
    public async Task<ActionResult> Update(
        [FromBody] CreateUpdateTenantDto createUpdateTenantDto)
    {
        if (!ModelState.IsValid)
        {
            var actionError = ModelState.GetErrors();
            return Error(actionError);
        }

        var result = await _tenantService.UpdateAsync(createUpdateTenantDto);
        return Ok(result);
    }

    /// <summary>
    /// 删除租户
    /// </summary>
    /// <param name="idCollection"></param>
    /// <returns></returns>
    [HttpDelete]
    [Route("delete")]
    [Description("Sys.Delete")]
    public async Task<ActionResult> Delete([FromBody] IdCollection idCollection)
    {
        if (!ModelState.IsValid)
        {
            var actionError = ModelState.GetErrors();
            return Error(actionError);
        }

        var result = await _tenantService.DeleteAsync(idCollection.IdArray);
        return Ok(result);
    }

    /// <summary>
    /// 获取租户列表
    /// </summary>
    /// <param name="tenantQueryCriteria"></param>
    /// <param name="pagination"></param>
    /// <returns></returns>
    [HttpGet]
    [Route("query")]
    [Description("Sys.Query")]
    public async Task<ActionResult> Query(TenantQueryCriteria tenantQueryCriteria, Pagination pagination)
    {
        var tenantList = await _tenantService.QueryAsync(tenantQueryCriteria, pagination);

        return JsonContent(tenantList, pagination);
    }

    /// <summary>
    /// 获取所有租户
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [Route("queryAll")]
    [Description("Action.GetAllTenant")]
    public async Task<ActionResult> QueryAll()
    {
        var tenantList = await _tenantService.QueryAllAsync();

        return JsonContent(tenantList);
    }


    /// <summary>
    /// 导出租户
    /// </summary>
    /// <param name="tenantQueryCriteria"></param>
    /// <returns></returns>
    [HttpGet]
    [Description("Sys.Export")]
    [Route("download")]
    public async Task<ActionResult> Download(TenantQueryCriteria tenantQueryCriteria)
    {
        var tenantExports = await _tenantService.DownloadAsync(tenantQueryCriteria);
        var data = new ExcelHelper().GenerateExcel(tenantExports, out var mimeType, out var fileName);
        return new FileContentResult(data, mimeType)
        {
            FileDownloadName = fileName
        };
    }

    #endregion
}
