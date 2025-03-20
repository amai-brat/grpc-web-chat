using Chat.Client;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Net.Client;

using var channel = GrpcChannel.ForAddress("http://localhost:5172");
var chatService = new ChatService.ChatServiceClient(channel);
var authService = new AuthService.AuthServiceClient(channel);
var ctSource = new CancellationTokenSource();

var tokenResponse = await authService.GetTokenAsync(new AuthorizeRequest
{
    Name = Guid.NewGuid().ToString()
}, cancellationToken: ctSource.Token);

var getCall = chatService.GetMessages(new Empty(), new Metadata
{
    { "Authorization", $"Bearer {tokenResponse.JwtToken}" }
}, cancellationToken: ctSource.Token);

var readTask = Task.Run(async () =>
{
    while (await getCall.ResponseStream.MoveNext(ctSource.Token))
    {
        var resp = getCall.ResponseStream.Current;
        Console.WriteLine($"{resp.SenderName}: {resp.Text}");
    } 
});


Console.WriteLine("Started, write message or :q to exit!");

while (true)
{
    var str = Console.ReadLine();
    if (str == ":q")
    {
        ctSource.Cancel();
        break;
    }

    await chatService.SendMessageAsync(new MessageRequest
    {
        Text = str
    }, headers: new Metadata
    {
        { "Authorization", $"Bearer {tokenResponse.JwtToken}" }
    }, cancellationToken: ctSource.Token);
}

await readTask;