namespace EventHub.Application.DTOs;

public class MfaSetupResponse
{
    public string SecretKey { get; set; } = string.Empty;
    public string QrCodeUri { get; set; } = string.Empty;
    public string[] RecoveryCodes { get; set; } = Array.Empty<string>();
}
