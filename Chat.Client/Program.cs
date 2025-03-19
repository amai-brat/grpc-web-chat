using Chat.Client;
using Grpc.Net.Client;

using var channel = GrpcChannel.ForAddress("http://localhost:5172");
var service = new ChatService.ChatServiceClient(channel);
var ctSource = new CancellationTokenSource();

var call = service.Chat();
var readTask = Task.Run(async () =>
{
    while (await call.ResponseStream.MoveNext(ctSource.Token))
    {
        var resp = call.ResponseStream.Current;
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
        await call.RequestStream.CompleteAsync();
        break;
    }

    await call.RequestStream.WriteAsync(new MessageRequest
    {
        Text = str
    }, ctSource.Token);
}

await readTask;