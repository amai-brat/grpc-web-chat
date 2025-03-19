using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;

namespace Auth.Service.Services;

public class AuthService(ITokenGenerator tokenGenerator) : Service.AuthService.AuthServiceBase
{
    private static readonly string Secret = Guid.NewGuid().ToString();

    public override Task<TokenResponse> GetToken(Empty request, ServerCallContext context)
    {
        var token = tokenGenerator.Generate();
        return Task.FromResult(new TokenResponse
        {
            JwtToken = token
        });
    }

    [Authorize]
    public override Task<SecretResponse> GetSecret(Empty request, ServerCallContext context)
    {
        
        return Task.FromResult(new SecretResponse
        {
            Secret = Secret
        });
    }
}