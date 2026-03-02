using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyWebApi.Application.Services;

namespace MyWebApi.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/vouchers")]
public sealed class VouchersController(IAccountingService accountingService) : ControllerBase
{
    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetById(
        long id,
        CancellationToken cancellationToken)
    {
        var voucher = await accountingService.GetVoucherByIdAsync(id, cancellationToken);
        return voucher is null ? NotFound() : Ok(voucher);
    }
}
