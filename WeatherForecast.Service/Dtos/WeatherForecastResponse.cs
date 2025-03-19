using System.Text.Json.Serialization;

namespace WeatherForecast.Service.Dtos;

public class Hourly
{
    [JsonPropertyName("time")]
    public List<long> Time { get; set; } = null!;

    [JsonPropertyName("temperature_2m")]
    public List<double> Temperature { get; set; } = null!;
}

public class HourlyUnits
{
    [JsonPropertyName("time")]
    public string Time { get; set; } = null!;

    [JsonPropertyName("temperature_2m")]
    public string Temperature { get; set; } = null!;
}

public class WeatherForecastResponse
{
    // public double latitude { get; set; }
    // public double longitude { get; set; }
    // public double generationtime_ms { get; set; }
    // public int utc_offset_seconds { get; set; }
    // public string timezone { get; set; }
    // public string timezone_abbreviation { get; set; }
    // public double elevation { get; set; }
    [JsonPropertyName("hourly_units")]
    public HourlyUnits HourlyUnits { get; set; } = null!;

    [JsonPropertyName("hourly")]
    public Hourly Hourly { get; set; } = null!;
}

