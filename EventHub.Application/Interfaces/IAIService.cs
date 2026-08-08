using EventHub.Domain.Entities;

namespace EventHub.Application.Interfaces;

/// <summary>
/// Application-layer contract for AI-powered event planning assistance.
/// Concrete implementations (MockAIService, GeminiAIService, etc.) live in
/// EventHub.Infrastructure — keeping the Domain and Application layers
/// free of infrastructure dependencies.
/// </summary>
public interface IAIService
{
    Task<string> GenerateResponseAsync(
        Event eventDetails,
        List<AIMessage> previousMessages,
        string userPrompt);
}
