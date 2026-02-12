using Newtonsoft.Json;

namespace SpotkaniaAPI.Models;

/// <summary>
/// Reprezentuje zarezerwowany termin spotkania
/// </summary>
public class BookedSlot
{
    [JsonProperty("date")]
    public string Date { get; set; } = string.Empty; // Format: YYYY-MM-DD

    [JsonProperty("time")]
    public string Time { get; set; } = string.Empty; // Format: HH:mm

    [JsonProperty("clientName")]
    public string ClientName { get; set; } = string.Empty;

    [JsonProperty("clientEmail")]
    public string ClientEmail { get; set; } = string.Empty;

    [JsonProperty("description")]
    public string? Description { get; set; }

    [JsonProperty("bookedAt")]
    public DateTime BookedAt { get; set; } = DateTime.UtcNow;
}
