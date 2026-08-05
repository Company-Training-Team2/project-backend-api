using EventHub.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;

namespace EventHub.Application.Services;

public class EmailService : IEmailService
{
    private readonly string _smtpHost;
    private readonly int _smtpPort;
    private readonly string _smtpUser;
    private readonly string _smtpPass;
    private readonly string _fromEmail;
    private readonly string _appName;

    public EmailService(IConfiguration config)
    {
        _smtpHost = config["Smtp:Host"] ?? "smtp.gmail.com";
        _smtpPort = int.Parse(config["Smtp:Port"] ?? "587");
        _smtpUser = config["Smtp:Username"] ?? string.Empty;
        _smtpPass = config["Smtp:Password"] ?? string.Empty;
        _fromEmail = config["Smtp:FromEmail"] ?? "noreply@eventhub.com";
        _appName = config["App:Name"] ?? "EventHub";
    }

    /// <summary>Sends 6-digit OTP code for email verification (audit Module 1).</summary>
    public async Task SendVerificationOtpAsync(string email, string code)
    {
        var subject = $"{_appName} — Verify your email";
        var body = $@"
            <h2>Email Verification</h2>
            <p>Your verification code is:</p>
            <h1 style='letter-spacing:8px'>{code}</h1>
            <p>This code expires in <strong>15 minutes</strong>.</p>
            <p>If you did not create an account, please ignore this email.</p>";

        await SendAsync(email, subject, body);
    }

    /// <summary>Sends 6-digit OTP code for password reset (audit Module 1).</summary>
    public async Task SendPasswordResetOtpAsync(string email, string code)
    {
        var subject = $"{_appName} — Password reset code";
        var body = $@"
            <h2>Password Reset</h2>
            <p>Your password reset code is:</p>
            <h1 style='letter-spacing:8px'>{code}</h1>
            <p>This code expires in <strong>15 minutes</strong>.</p>
            <p>If you did not request this, please ignore this email.</p>";

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