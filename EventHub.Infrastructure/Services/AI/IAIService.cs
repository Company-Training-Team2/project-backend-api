using EventHub.Domain.Entities;

namespace EventHub.Infrastructure.Services.AI;

public interface IAIService
{
    Task<string> GenerateResponseAsync(
        Event eventDetails,
        List<AIMessage> previousMessages,
        string userPrompt);
}