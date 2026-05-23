using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TurnOS.API.Extensions;
using TurnOS.Application.DTOs.Service;
using TurnOS.Application.Interfaces;

namespace TurnOS.API.Controllers;

[ApiController]
[Route("api/services")]
[Authorize(Roles = "BusinessOwner,Admin")]
public class ServiceController : ControllerBase
{
    private readonly IServiceService _serviceService;

    public ServiceController(IServiceService serviceService) =>
        _serviceService = serviceService;

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateServiceRequest request)
    {
        var requestingUserId = User.GetUserId();
        var service = await _serviceService.CreateAsync(request, requestingUserId);
        return CreatedAtAction(nameof(Create), new { id = service.Id }, service);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateServiceRequest request)
    {
        var requestingUserId = User.GetUserId();
        var service = await _serviceService.UpdateAsync(id, request, requestingUserId);
        return Ok(service);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var requestingUserId = User.GetUserId();
        await _serviceService.DeleteAsync(id, requestingUserId);
        return NoContent();
    }
}
