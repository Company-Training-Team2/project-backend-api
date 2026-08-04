using EventHub.Application.DTOs;

namespace EventHub.Application.Interfaces;

public interface IMfaService
{
    MfaSetupResponse GenerateSetup(string email);
    bool ValidateCode(string secret, string code);
    string[] GenerateRecoveryCodes();
}
