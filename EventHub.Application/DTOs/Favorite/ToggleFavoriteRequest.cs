namespace EventHub.Application.DTOs.Favorite;

/// <summary>Audit Module 9: POST /api/favorites/toggle request body.</summary>
public class ToggleFavoriteRequest
{
    public int WorkPostId { get; set; }
}
