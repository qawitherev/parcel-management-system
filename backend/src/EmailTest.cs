using System;
using System.Threading.Tasks;
using System.Net;
using System.Net.Mail;

public class EmailNotificationSender
{
    private readonly string _smtpHost;
    private readonly int _smtpPort;
    private readonly string _username;
    private readonly string _password;
    private readonly string _fromAddress;

    public EmailNotificationSender(string smtpHost, int smtpPort, string username, string password, string fromAddress)
    {
        _smtpHost = smtpHost;
        _smtpPort = smtpPort;
        _username = username;
        _password = password;
        _fromAddress = fromAddress;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string body)
    {
        Console.WriteLine($"[Testing] Sending email to {toEmail} via {_smtpHost}...");
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
        Console.WriteLine("[Success] Email sent successfully!");
    }
}

class Program
{
    static async Task Main(string[] args)
    {
        if (args.Length < 1)
        {
            Console.WriteLine("Please provide a recipient email address: dotnet run <email>");
            return;
        }

        string recipient = args[0];
        
        // Credentials from .env
        string smtpHost = "smtp.sendgrid.net";
        int smtpPort = 587;
        string username = "apikey";
        string password = Environment.GetEnvironmentVariable("SENDGRID_API_KEY") ?? throw new InvalidOperationException("SENDGRID_API_KEY environment variable not set");
        string fromAddress = "noreply@qawitherev.com";

        var sender = new EmailNotificationSender(smtpHost, smtpPort, username, password, fromAddress);
        
        try 
        {
            await sender.SendEmailAsync(recipient, "Test: Parcel Check-in Notification", "This is a test email to verify the SendGrid SMTP configuration is working correctly.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Error] Failed to send email: {ex.Message}");
            if (ex.InnerException != null) 
            {
                Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
            }
        }
    }
}
