using System.Collections.Concurrent;
using Grpc.Core;

namespace Chat.Service.Services;

public class AuthService(
    ITokenGenerator tokenGenerator) : Service.AuthService.AuthServiceBase
{
    private static readonly ConcurrentBag<string> Users = [];
    public override Task<TokenResponse> GetToken(AuthorizeRequest request, ServerCallContext context)
    {
        if (Users.Contains(request.Name))
            throw new RpcException(new Status(StatusCode.Unauthenticated, "Not unique name"));

        Users.Add(request.Name);
        
        return Task.FromResult(new TokenResponse
        {
            JwtToken = tokenGenerator.Generate(request.Name)
        });
    }
}