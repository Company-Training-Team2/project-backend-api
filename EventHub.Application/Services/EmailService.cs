using EventHub.Application.Interfaces;
using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;

namespace EventHub.Application.Services;

public class EmailService : IEmailService
{
    private readonly string _smtpHost;
    private readonly int _smtpPort;
    private readonly string _smtpUser;
    private readonly string _smtpPass;
    private readonly string _fromEmail;

    public EmailService(IConfiguration config)
    {
        _smtpHost = config["Smtp:Host"] ?? "smtp.gmail.com";
        _smtpPort = int.Parse(config["Smtp:Port"] ?? "587");
        _smtpUser = config["Smtp:Username"] ?? "";
        _smtpPass = config["Smtp:Password"] ?? "";
        _fromEmail = config["Smtp:FromEmail"] ?? "noreply@eventhub.com";
    }

    public async Task SendVerificationEmailAsync(string email, string token)
    {
        var subject = "Verify your EventHub account";
        var body = $"Click to verify: https://yourfrontend.com/verify?token={token}";
        await SendAsync(email, subject, body);
    }

    public async Task SendPasswordResetEmailAsync(string email, string token)
    {
        var subject = "Reset your EventHub password";
        var body = $"Click to reset: https://yourfrontend.com/reset-password?token={token}";
        await SendAsync(email, subject, body);
    }

    private async Task SendAsync(string to, string subject, string body)
    {
        using var client = new SmtpClient(_smtpHost, _smtpPort)
        {
            EnableSsl = true,
            Credentials = new NetworkCredential(_smtpUser, _smtpPass)
        };

        var message = new MailMessage(_fromEmail, to, subject, body)
        {
            IsBodyHtml = true
        };

        await client.SendMailAsync(message);
    }
}
