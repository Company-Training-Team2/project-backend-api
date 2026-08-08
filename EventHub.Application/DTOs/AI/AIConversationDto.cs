namespace EventHub.Application.DTOs.AI;

public class AIConversationDto
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public List<AIMessageDto> Messages { get; set; } = new();
}
