using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyWebApi.Application.Auth;
using MyWebApi.Application.Contracts;
using MyWebApi.Application.Services;

namespace MyWebApi.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/payments")]
public sealed class PaymentsController(IAccountingService accountingService) : ControllerBase
{
    [HttpPost]
    [Authorize(Policy = AuthorizationConstants.Policies.CanInitiateAp)]
    public async Task<IActionResult> CreateDraft(
        [FromBody] PaymentCreateRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var id = await accountingService.CreatePaymentDraftAsync(
                request,
                GetUserId(),
                cancellationToken);

            return Created($"/api/v1/payments/{id}", new { id });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id:long}/post")]
    [Authorize(Policy = AuthorizationConstants.Policies.CanAuthoriseAp)]
    public async Task<IActionResult> Post(
        long id,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await accountingService.PostPaymentAsync(id, GetUserId(), cancellationToken);
            if (!result.Found)
            {
                return NotFound();
            }

            return Ok(new AccountingPostResponse(
                VoucherId: result.VoucherId,
                VoucherNo: result.VoucherNo,
                AlreadyPosted: result.AlreadyPosted));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    private int GetUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(value, out var userId) ? userId : 0;
    }
}
