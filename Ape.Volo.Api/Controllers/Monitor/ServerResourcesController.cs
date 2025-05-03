using System.ComponentModel;
using System.Threading.Tasks;
using Ape.Volo.Api.Controllers.Base;
using Ape.Volo.Common.Attributes;
using Ape.Volo.IBusiness.Monitor;
using Ape.Volo.ViewModel.ServerInfo;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Ape.Volo.Api.Controllers.Monitor;

/// <summary>
/// 服务器管理
/// </summary>
[Area("Area.ServerResourceManagement")]
[Route("/api/service", Order = 16)]
public class ServerResourcesController : BaseApiController
{
    private readonly IServerResourcesService _serverResourcesService;

    public ServerResourcesController(IServerResourcesService serverResourcesService)
    {
        _serverResourcesService = serverResourcesService;
    }

    #region 对内接口

    [HttpGet]
    [Route("resources/info")]
    [Description("Action.ServerResourceInfo")]
    [NotAudit]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ServerResourcesInfo))]
    public async Task<ActionResult> Query()
    {
        var resourcesInfo = await _serverResourcesService.Query();

        return JsonContent(resourcesInfo);
    }

    #endregion
}
