using Newtonsoft.Json;

namespace SpotkaniaAPI.Models;

/// <summary>
/// Reprezentuje osobę, z którą można umawiać spotkania
/// </summary>
public class Person
{
    [JsonProperty("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;

    [JsonProperty("email")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Słownik z godzinami pracy dla każdego dnia tygodnia
    /// Klucze: Monday, Tuesday, Wednesday, Thursday, Friday, Saturday, Sunday
    /// </summary>
    [JsonProperty("workHours")]
    public Dictionary<string, WorkDay> WorkHours { get; set; } = new()
    {
        { "Monday", new WorkDay { Start = "08:00", End = "16:00", Enabled = true } },
        { "Tuesday", new WorkDay { Start = "08:00", End = "16:00", Enabled = true } },
        { "Wednesday", new WorkDay { Start = "08:00", End = "16:00", Enabled = true } },
        { "Thursday", new WorkDay { Start = "08:00", End = "16:00", Enabled = true } },
        { "Friday", new WorkDay { Start = "08:00", End = "16:00", Enabled = true } },
        { "Saturday", new WorkDay { Start = "09:00", End = "14:00", Enabled = false } },
        { "Sunday", new WorkDay { Start = "10:00", End = "14:00", Enabled = false } }
    };

    /// <summary>
    /// Lista zarezerwowanych terminów
    /// </summary>
    [JsonProperty("bookedSlots")]
    public List<BookedSlot> BookedSlots { get; set; } = new();

    [JsonProperty("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
