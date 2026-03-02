using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyWebApi.Application.Services;

namespace MyWebApi.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/gl")]
public sealed class GlController(IAccountingService accountingService) : ControllerBase
{
    [HttpGet("accounts")]
    public async Task<IActionResult> GetAccounts(CancellationToken cancellationToken)
    {
        var data = await accountingService.GetLedgerAccountsAsync(cancellationToken);
        return Ok(data);
    }

    [HttpGet("trial-balance")]
    public async Task<IActionResult> GetTrialBalance(
        [FromQuery] DateOnly from,
        [FromQuery] DateOnly to,
        CancellationToken cancellationToken)
    {
        try
        {
            var data = await accountingService.GetTrialBalanceAsync(from, to, cancellationToken);
            return Ok(data);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("ledger/{accountId:long}")]
    public async Task<IActionResult> GetLedger(
        long accountId,
        [FromQuery] DateOnly from,
        [FromQuery] DateOnly to,
        CancellationToken cancellationToken)
    {
        try
        {
            var data = await accountingService.GetLedgerAsync(accountId, from, to, cancellationToken);
            return Ok(data);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
