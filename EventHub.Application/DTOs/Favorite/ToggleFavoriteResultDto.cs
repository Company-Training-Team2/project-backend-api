namespace EventHub.Application.DTOs.Favorite;

/// <summary>
/// Audit Module 9: result of a toggle — tells the client (Heart icon on
/// Vendor Cards / Search views) the new state so it can update in place.
/// </summary>
public class ToggleFavoriteResultDto
{
    public int WorkPostId { get; set; }

    public bool IsFavorited { get; set; }
}
