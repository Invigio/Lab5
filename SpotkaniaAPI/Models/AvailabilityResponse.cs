namespace SpotkaniaAPI.Models;

/// <summary>
/// Odpowiedź z dostępnymi slotami
/// </summary>
public class AvailabilityResponse
{
    public string PersonId { get; set; } = string.Empty;
    public string PersonName { get; set; } = string.Empty;
    public string Date { get; set; } = string.Empty;
    public string DayOfWeek { get; set; } = string.Empty;
    public List<string> AvailableSlots { get; set; } = new();
}
