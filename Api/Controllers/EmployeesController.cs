using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using MyWebApi.Application.Contracts;
using MyWebApi.Application.Services;

namespace MyWebApi.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/employees")]
public sealed class EmployeesController(IEmployeeService employeeService) : ControllerBase
{
    [HttpGet]
    [OutputCache(Duration = 30)]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var data = await employeeService.GetPagedAsync(page, pageSize, cancellationToken);
        return Ok(data);
    }

    [HttpGet("{id:int}")]
    [OutputCache(Duration = 30)]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var employee = await employeeService.GetByIdAsync(id, cancellationToken);
        return employee is null ? NotFound() : Ok(employee);
    }
    [AllowAnonymous]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] EmployeeCreateRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var createdId = await employeeService.CreateAsync(request, cancellationToken);
            return Created($"/api/v1/employees/{createdId}", new { id = createdId });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] EmployeeUpdateRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var updated = await employeeService.UpdateAsync(id, request, cancellationToken);
            return updated ? NoContent() : NotFound();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Deactivate(int id, CancellationToken cancellationToken)
    {
        var deactivated = await employeeService.DeactivateAsync(id, cancellationToken);
        return deactivated ? NoContent() : NotFound();
    }
}
