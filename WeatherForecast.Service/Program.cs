using WeatherForecast.Service.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient("Open-meteo", 
    client => client.BaseAddress = new Uri("https://api.open-meteo.com"));

builder.Services.AddGrpc();

var app = builder.Build();

app.MapGrpcService<GreeterService>();
app.MapGrpcService<WeatherForecastService>();

app.Run();