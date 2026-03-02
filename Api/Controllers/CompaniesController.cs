using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using MyWebApi.Application.Contracts;
using MyWebApi.Application.Services;

namespace MyWebApi.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/companies")]
public sealed class CompaniesController(ICompanyService companyService)
    : ControllerBase
{
    /// <summary>
    /// Get all companies (usually only one in ERP)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(
        CancellationToken cancellationToken)
    {
        var data = await companyService.GetAllAsync(cancellationToken);
        return Ok(data);
    }
    /// <summary>
    /// Create company (ERP setup – FIRST STEP)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CompanyCreateRequest request,
        CancellationToken cancellationToken)
    {
        var companyId =
            await companyService.CreateAsync(request, cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { id = companyId },
            new { CompanyId = companyId }
        );
    }

    /// <summary>
    /// Get company by id
    /// </summary>
    [HttpGet("{id:long}")]
    [OutputCache(Duration = 30)]
    public async Task<IActionResult> GetById(
        long id,
        CancellationToken cancellationToken)
    {
        // Implement GetByIdAsync in service when needed
        return Ok(new { Id = id });
    }

    /// <summary>
    /// Deactivate company (soft delete)
    /// </summary>
    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Deactivate(
        long id,
        CancellationToken cancellationToken)
    {
        // Implement DeactivateAsync in service when needed
        return NoContent();
    }
}
