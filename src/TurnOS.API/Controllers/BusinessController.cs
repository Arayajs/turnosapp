using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TurnOS.API.Extensions;
using TurnOS.Application.DTOs.Business;
using TurnOS.Application.Interfaces;

namespace TurnOS.API.Controllers;

[ApiController]
[Route("api/businesses")]
public class BusinessController : ControllerBase
{
    private readonly IBusinessService _businessService;

    public BusinessController(IBusinessService businessService) =>
        _businessService = businessService;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var businesses = await _businessService.GetAllAsync();
        return Ok(businesses);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var business = await _businessService.GetByIdAsync(id);
        return Ok(business);
    }

    [HttpPost]
    [Authorize(Roles = "BusinessOwner,Admin")]
    public async Task<IActionResult> Create([FromBody] CreateBusinessRequest request)
    {
        var ownerId = User.GetUserId();
        var business = await _businessService.CreateAsync(request, ownerId);
        return CreatedAtAction(nameof(GetById), new { id = business.Id }, business);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "BusinessOwner,Admin")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateBusinessRequest request)
    {
        var requestingUserId = User.GetUserId();
        var business = await _businessService.UpdateAsync(id, request, requestingUserId);
        return Ok(business);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "BusinessOwner,Admin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var requestingUserId = User.GetUserId();
        await _businessService.DeleteAsync(id, requestingUserId);
        return NoContent();
    }

    [HttpGet("{id:guid}/services")]
    public async Task<IActionResult> GetServices(Guid id)
    {
        var services = await _businessService.GetServicesAsync(id);
        return Ok(services);
    }
}
