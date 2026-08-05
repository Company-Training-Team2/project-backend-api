namespace EventHub.Application.Interfaces;

public interface IEmailService
{
    /// <summary>Sends 6-digit OTP for email verification (audit Module 1).</summary>
    Task SendVerificationOtpAsync(string email, string code);

    /// <summary>Sends 6-digit OTP for password reset (audit Module 1).</summary>
    Task SendPasswordResetOtpAsync(string email, string code);
}