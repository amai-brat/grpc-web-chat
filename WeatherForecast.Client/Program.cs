using Grpc.Core;
using Grpc.Net.Client;
using WeatherForecast.Client;

using var channel = GrpcChannel.ForAddress("http://localhost:5174");
var client = new WeatherForecastService.WeatherForecastServiceClient(channel);
var call = client.GetForecast(new ForecastRequest());

while (await call.ResponseStream.MoveNext())
{
    var time = call.ResponseStream.Current.DateTime.ToDateTime();
    var temperature = call.ResponseStream.Current.Temperature;
    Console.WriteLine($"Погода на {time:dd-MM-yyyy HH:mm} = {temperature}");    
}