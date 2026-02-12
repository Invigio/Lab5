using Newtonsoft.Json;

namespace SpotkaniaAPI.Models;

/// <summary>
/// Reprezentuje godziny pracy w danym dniu
/// </summary>
public class WorkDay
{
    [JsonProperty("start")]
    public string Start { get; set; } = string.Empty;

    [JsonProperty("end")]
    public string End { get; set; } = string.Empty;

    [JsonProperty("enabled")]
    public bool Enabled { get; set; } = true;
}
