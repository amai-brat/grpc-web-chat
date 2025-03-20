using System.Text;
using Chat.Service.Options;
using Chat.Service.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("JwtOptions"));

builder.Services.AddScoped<ITokenGenerator, TokenGenerator>();
builder.Services.AddGrpc();
builder.Services.AddAuthorization();
builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["JwtOptions:Key"] 
                                       ?? throw new InvalidOperationException("Security key could not be found"))),
        };
    });

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapGrpcService<ChatServiceV4>();
app.MapGrpcService<AuthService>();
app.Run();