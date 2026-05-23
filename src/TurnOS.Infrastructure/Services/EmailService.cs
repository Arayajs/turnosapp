using Microsoft.Extensions.Configuration;
using SendGrid;
using SendGrid.Helpers.Mail;
using TurnOS.Domain.Entities;
using TurnOS.Domain.Interfaces;

namespace TurnOS.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly string _apiKey;
    private readonly string _fromEmail;
    private readonly string _fromName;

    public EmailService(IConfiguration configuration)
    {
        _apiKey = configuration["SendGrid:ApiKey"] ?? string.Empty;
        _fromEmail = configuration["SendGrid:FromEmail"] ?? "noreply@turnos.app";
        _fromName = configuration["SendGrid:FromName"] ?? "TurnOS";
    }

    public async Task SendConfirmationAsync(string toEmail, Appointment appointment)
    {
        var subject = $"Cita confirmada — {appointment.StartTime:dddd dd/MM HH:mm}";
        var body = $"Tu cita para <strong>{appointment.Service?.Name}</strong> está confirmada.<br>" +
                   $"Fecha: {appointment.StartTime:dddd dd/MM/yyyy HH:mm}<br>" +
                   $"Negocio: {appointment.Business?.Name}";
        await SendEmailAsync(toEmail, subject, body);
    }

    public async Task SendCancellationAsync(string toEmail, Appointment appointment)
    {
        var subject = "Cita cancelada";
        var body = $"Tu cita para <strong>{appointment.Service?.Name}</strong> el " +
                   $"{appointment.StartTime:dd/MM/yyyy HH:mm} ha sido cancelada.";
        await SendEmailAsync(toEmail, subject, body);
    }

    public async Task SendPasswordResetAsync(string toEmail, string resetToken)
    {
        var subject = "Recuperar contraseña — TurnOS";
        var body = $"Usa el siguiente token para restablecer tu contraseña: <strong>{resetToken}</strong><br>" +
                   "Este enlace expira en 1 hora.";
        await SendEmailAsync(toEmail, subject, body);
    }

    private async Task SendEmailAsync(string toEmail, string subject, string htmlBody)
    {
        if (string.IsNullOrEmpty(_apiKey) || _apiKey == "tu-sendgrid-api-key")
            return;

        var client = new SendGridClient(_apiKey);
        var from = new EmailAddress(_fromEmail, _fromName);
        var to = new EmailAddress(toEmail);
        var msg = MailHelper.CreateSingleEmail(from, to, subject, null, htmlBody);
        await client.SendEmailAsync(msg);
    }
}
