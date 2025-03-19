using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Chat.Service.Models;
using Grpc.Core;

namespace Chat.Service.Services;

public class ChatService : Service.ChatService.ChatServiceBase
{
    private static readonly ObservableCollection<Message> Messages = [];
    private static readonly SemaphoreSlim Semaphore = new(1, 1);
    
    public override async Task Chat(
        IAsyncStreamReader<MessageRequest> requestStream, 
        IServerStreamWriter<MessageResponse> responseStream, 
        ServerCallContext context)
    {
        Console.WriteLine(context.Peer);
        
        NotifyCollectionChangedEventHandler handler = async void (_, args) =>
        {
            var messages = args.NewItems!.OfType<Message>();
            foreach (var message in messages.Select(x => new MessageResponse
                     {
                         SenderName = x.SenderName,
                         Text = x.Text
                     }))
            {
                await responseStream.WriteAsync(message, context.CancellationToken);
            }
        };

        Messages.CollectionChanged += handler; 
        context.CancellationToken.Register(() =>
        {
            Messages.CollectionChanged -= handler;
        });
        var readTask = Task.Run(async () =>
        {
            while (await requestStream.MoveNext(context.CancellationToken))
            {
                var req = requestStream.Current;
                var message = new Message
                {
                    // TODO: Authorize
                    Id = Guid.NewGuid(),
                    CreatedAt = DateTime.UtcNow,
                    SenderName = "ABOBA",
                    Text = req.Text,
                };

                await Semaphore.WaitAsync(context.CancellationToken);

                Messages.Add(message);

                Semaphore.Release();
            }
        }, context.CancellationToken);

        await readTask;
        Messages.CollectionChanged -= handler;
    }
}