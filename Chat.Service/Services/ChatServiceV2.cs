using System.Threading.Channels;
using Chat.Service.Models;
using Grpc.Core;

namespace Chat.Service.Services;

public class ChatServiceV2 : Service.ChatService.ChatServiceBase
{
    private static readonly List<Message> History = [];
    private static readonly ReaderWriterLockSlim RwLock = new(); 
    
    private static readonly Channel<Message> Channel = System.Threading.Channels.Channel.CreateUnbounded<Message>();
    public override async Task Chat(
        IAsyncStreamReader<MessageRequest> requestStream, 
        IServerStreamWriter<MessageResponse> responseStream, 
        ServerCallContext context)
    {
        await LoadHistory(responseStream, context);
        
        var receiveTask = ReceiveMessages(requestStream, context);
        var sendTask = SendMessages(responseStream, context);
        
        await Task.WhenAny(receiveTask, sendTask);
    }

    private static async Task LoadHistory(
        IServerStreamWriter<MessageResponse> responseStream,
        ServerCallContext context)
    {
        RwLock.EnterReadLock();
        try
        {
            foreach (var message in History)
            {
                await responseStream.WriteAsync(message.ToResponse(), context.CancellationToken);
            }
        }
        finally
        {
            RwLock.ExitReadLock();
        }
    }
    
    private static async Task ReceiveMessages(
        IAsyncStreamReader<MessageRequest> requestStream,
        ServerCallContext context)
    {
        await foreach (var req in requestStream.ReadAllAsync(context.CancellationToken))
        {
            var message = Message.From(req, "ABOBA");
            
            RwLock.EnterWriteLock();
            try
            {
                History.Add(message);
            }
            finally
            {
                RwLock.ExitWriteLock();
            }
            
            await Channel.Writer.WriteAsync(message, context.CancellationToken);
        }
    }

    private static async Task SendMessages(
        IServerStreamWriter<MessageResponse> responseStream,
        ServerCallContext context)
    {
        await foreach (var message in Channel.Reader.ReadAllAsync(context.CancellationToken))
        {
            try
            {
                await responseStream.WriteAsync(message.ToResponse());
            }
            catch (RpcException ex) when (ex.StatusCode == StatusCode.Cancelled)
            {
                Console.WriteLine("CANCELLED");
                break;
            }
        }
    }
}