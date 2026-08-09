using EventHub.Application.DTOs.Auth;
using EventHub.Application.Interfaces;
using OtpNet;

namespace EventHub.Application.Services;

public class MfaService : IMfaService
{
    public MfaSetupResponse GenerateSetup(string email)
    {
        var secret = KeyGeneration.GenerateRandomKey(20);
        var base32Secret = Base32Encoding.ToString(secret);
        var issuer = "EventHub";
        var qrUri = $"otpauth://totp/{issuer}:{email}?secret={base32Secret}&issuer={issuer}";

        return new MfaSetupResponse
        {
            SecretKey = base32Secret,
            QrCodeUri = qrUri,
            RecoveryCodes = GenerateRecoveryCodes()
        };
    }

    public bool ValidateCode(string secret, string code)
    {
        var totp = new Totp(Base32Encoding.ToBytes(secret));
        return totp.VerifyTotp(code, out _, VerificationWindow.RfcSpecifiedNetworkDelay);
    }

    public string[] GenerateRecoveryCodes()
    {
        var codes = new string[10];
        for (int i = 0; i < 10; i++)
            codes[i] = Guid.NewGuid().ToString("N")[..8].ToUpper();
        return codes;
    }
}
