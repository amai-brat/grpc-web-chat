using Auth.Client;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Net.Client;

using var channel = GrpcChannel.ForAddress("http://localhost:5262");
var client = new AuthService.AuthServiceClient(channel);

var resp = client.GetToken(new Empty());
Console.WriteLine($"JWT: {resp}");

var secret = client.GetSecret(new Empty(), new Metadata
{
    { "Authorization", "Bearer " + resp.JwtToken }
});

Console.WriteLine($"SECRET: {secret}");