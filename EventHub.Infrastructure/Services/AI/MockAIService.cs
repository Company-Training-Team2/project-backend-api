using EventHub.Domain.Entities;

namespace EventHub.Infrastructure.Services.AI;

public class MockAIService : IAIService
{
    public Task<string> GenerateResponseAsync(
        Event eventDetails,
        List<AIMessage> previousMessages,
        string userPrompt)
    {
        userPrompt = userPrompt.ToLower();

        string response;

        if (userPrompt.Contains("budget"))
        {
            response =
                $"Your total budget is {eventDetails.TotalBudget:C}. " +
                "I recommend allocating 40% for venue, 30% for catering, " +
                "15% for photography and keeping 15% as contingency.";
        }
        else if (userPrompt.Contains("vendor"))
        {
            response =
                "You can start by booking your venue first, then photographer, " +
                "catering, decorations and entertainment.";
        }
        else if (userPrompt.Contains("timeline"))
        {
            response =
                "Based on your event date, begin with venue booking, " +
                "then invitations, followed by payment confirmations.";
        }
        else
        {
            response =
                $"I received your request: \"{userPrompt}\".\n" +
                "This is a mock AI response. " +
                "Later this service will be replaced by Google Gemini without changing the controller.";
        }

        return Task.FromResult(response);
    }
}