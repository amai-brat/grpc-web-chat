using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using WeatherForecast.Service.Dtos;

namespace WeatherForecast.Service.Services;

public class WeatherForecastService(IHttpClientFactory httpClientFactory) : Service.WeatherForecastService.WeatherForecastServiceBase
{
    private const string WeatherUrl = "/v1/forecast?" +
                                      "latitude=55.7887&" +
                                      "longitude=49.1221&" +
                                      "hourly=temperature_2m&" +
                                      "timezone=Europe%2FMoscow&" +
                                      "timeformat=unixtime";
    
    private readonly TimeSpan _delayBetweenStreamElements = TimeSpan.FromSeconds(1); 
    private readonly HttpClient _httpClient = httpClientFactory.CreateClient("Open-meteo");
    
    public override async Task GetForecast(ForecastRequest request, IServerStreamWriter<ForecastResponse> responseStream, ServerCallContext context)
    {
        var resp = await _httpClient.GetFromJsonAsync<WeatherForecastResponse>(WeatherUrl);
        if (resp is null)
        {
            throw new RpcException(new Status(StatusCode.Internal, "Weather forecast could not be retrieved"));
        }

        var temperatureUnit = resp.HourlyUnits.Temperature;
        
        foreach (var (time, temperature) in resp.Hourly.Time
                     .Zip(resp.Hourly.Temperature)
                     .Where((_, i) => i % 2 == 0)) // каждые два часа
        {
            await responseStream.WriteAsync(new ForecastResponse
            {
                DateTime = new Timestamp { Seconds = time },
                Temperature = $"{temperature}{temperatureUnit}"
            });
            
            await Task.Delay(_delayBetweenStreamElements, context.CancellationToken);
        }
    }
}