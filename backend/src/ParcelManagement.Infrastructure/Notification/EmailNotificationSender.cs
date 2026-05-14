using ParcelManagement.Core.Services;
using System.Net;
using System.Net.Mail;

namespace ParcelManagement.Infrastructure.Notification;

/// <summary>
/// Sends email notifications via SMTP (SendGrid-compatible).
/// WhatsApp is deferred — requires a PhoneNumber field on the User entity.
/// </summary>
public class EmailNotificationSender : INotificationSender
{
    private readonly string _smtpHost;
    private readonly int _smtpPort;
    private readonly string _username;
    private readonly string _password;
    private readonly string _fromAddress;

    public EmailNotificationSender(
        string smtpHost, int smtpPort,
        string username, string password, string fromAddress)
    {
        _smtpHost = smtpHost;
        _smtpPort = smtpPort;
        _username = username;
        _password = password;
        _fromAddress = fromAddress;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string body)
    {
        using var client = new SmtpClient(_smtpHost, _smtpPort)
        {
            Credentials = new NetworkCredential(_username, _password),
            EnableSsl = true
        };

        var mail = new MailMessage(_fromAddress, toEmail, subject, body)
        {
            IsBodyHtml = false
        };

        await client.SendMailAsync(mail);
    }

    public Task SendWhatsAppAsync(string toPhone, string message)
    {
        // Not yet implemented — awaiting User.PhoneNumber field
        Console.WriteLine(
            $"[EmailNotificationSender] WhatsApp skipped: User entity has no phone number field.");
        return Task.CompletedTask;
    }
}
