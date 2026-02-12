namespace SpotkaniaAPI.Models;

/// <summary>
/// Request do rezerwacji spotkania
/// </summary>
public class BookAppointmentRequest
{
    public string PersonId { get; set; } = string.Empty;
    public string Date { get; set; } = string.Empty; // Format: YYYY-MM-DD
    public string Time { get; set; } = string.Empty; // Format: HH:mm
    public string ClientName { get; set; } = string.Empty;
    public string ClientEmail { get; set; } = string.Empty;
    public string? Description { get; set; }
}
