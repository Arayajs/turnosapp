using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TurnOS.API.Extensions;
using TurnOS.Application.DTOs.Appointment;
using TurnOS.Application.Interfaces;

namespace TurnOS.API.Controllers;

[ApiController]
[Route("api/appointments")]
public class AppointmentController : ControllerBase
{
    private readonly IAppointmentService _appointmentService;

    public AppointmentController(IAppointmentService appointmentService) =>
        _appointmentService = appointmentService;

    [HttpGet("available-slots")]
    public async Task<IActionResult> GetAvailableSlots(
        [FromQuery] Guid businessId,
        [FromQuery] Guid serviceId,
        [FromQuery] DateOnly date)
    {
        var slots = await _appointmentService.GetAvailableSlotsAsync(businessId, serviceId, date);
        return Ok(slots);
    }

    [HttpPost]
    [Authorize(Roles = "Client,Admin")]
    public async Task<IActionResult> Create([FromBody] CreateAppointmentRequest request)
    {
        var clientId = User.GetUserId();
        var appointment = await _appointmentService.CreateAsync(request, clientId);
        return CreatedAtAction(nameof(GetMyAppointments), appointment);
    }

    [HttpGet("my")]
    [Authorize(Roles = "Client,Admin")]
    public async Task<IActionResult> GetMyAppointments()
    {
        var clientId = User.GetUserId();
        var appointments = await _appointmentService.GetMyAppointmentsAsync(clientId);
        return Ok(appointments);
    }

    [HttpGet("business/{businessId:guid}")]
    [Authorize(Roles = "BusinessOwner,Admin")]
    public async Task<IActionResult> GetBusinessAppointments(
        Guid businessId, [FromQuery] DateOnly? date = null)
    {
        var requestingUserId = User.GetUserId();
        var appointments = await _appointmentService.GetBusinessAppointmentsAsync(
            businessId, requestingUserId, date);
        return Ok(appointments);
    }

    [HttpPatch("{id:guid}/status")]
    [Authorize(Roles = "BusinessOwner,Admin")]
    public async Task<IActionResult> UpdateStatus(
        Guid id, [FromBody] UpdateAppointmentStatusRequest request)
    {
        var requestingUserId = User.GetUserId();
        var appointment = await _appointmentService.UpdateStatusAsync(id, request, requestingUserId);
        return Ok(appointment);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Client,Admin")]
    public async Task<IActionResult> Cancel(Guid id)
    {
        var clientId = User.GetUserId();
        await _appointmentService.CancelAsync(id, clientId);
        return NoContent();
    }
}
