using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyWebApi.Application.Services;

namespace MyWebApi.Api.Controllers;

[ApiController]
//[Authorize(Roles = "Admin")]
[Route("api/v1/admin")]
public sealed class AdminController(IAdminService service) : ControllerBase
{
    [HttpGet("login-summary")]
    public async Task<IActionResult> GetLoginSummary(
        CancellationToken cancellationToken)
    {
        var data = await service.GetLoginSummaryAsync(cancellationToken);
        return Ok(data);
    }

    [HttpGet("active-users")]
    public async Task<IActionResult> GetActiveUsers(
        CancellationToken cancellationToken)
    {
        var data = await service.GetActiveUsersAsync(cancellationToken);
        return Ok(data);
    }

    [HttpGet("user/{id:int}/sessions")]
    public async Task<IActionResult> GetUserSessions(
        int id,
        CancellationToken cancellationToken)
    {
        var data = await service.GetUserSessionsAsync(id, cancellationToken);
        return Ok(data);
    }
}
