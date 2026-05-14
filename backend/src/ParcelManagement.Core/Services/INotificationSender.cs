namespace ParcelManagement.Core.Services;

/// <summary>
/// Abstraction for sending notifications to residents.
/// Core defines WHAT to send; Infrastructure defines HOW (email, WhatsApp, etc.).
/// </summary>
public interface INotificationSender
{
    Task SendEmailAsync(string toEmail, string subject, string body);
    Task SendWhatsAppAsync(string toPhone, string message);
}
