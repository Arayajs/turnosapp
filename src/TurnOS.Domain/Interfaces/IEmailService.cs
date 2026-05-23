using TurnOS.Domain.Entities;

namespace TurnOS.Domain.Interfaces;

public interface IEmailService
{
    Task SendConfirmationAsync(string toEmail, Appointment appointment);
    Task SendCancellationAsync(string toEmail, Appointment appointment);
    Task SendPasswordResetAsync(string toEmail, string resetToken);
}
