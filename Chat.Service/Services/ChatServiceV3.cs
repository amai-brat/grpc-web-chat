using System.Collections.Concurrent;
using System.Security.Claims;
using Chat.Service.Models;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;

namespace Chat.Service.Services;

public class ChatServiceV3 : Service.ChatService.ChatServiceBase
{
    private static readonly ConcurrentDictionary<string, IServerStreamWriter<MessageResponse>> Clients = new();
    private static readonly List<Message> History = [];
    private static readonly ReaderWriterLockSlim RwLock = new();

    [Authorize]
    public override async Task Chat(
        IAsyncStreamReader<MessageRequest> requestStream, 
        IServerStreamWriter<MessageResponse> responseStream, 
        ServerCallContext context)
    {
        var clientId = context.Peer;
        if (!Clients.TryAdd(clientId, responseStream))
            throw new RpcException(new Status(StatusCode.Cancelled, "Failed to add client"));

        await LoadHistory(responseStream, context);

        try
        {
            await HandleMessages(requestStream, context);
        }
        finally
        {
            Clients.TryRemove(clientId, out _);
        }
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

    private static async Task HandleMessages(
        IAsyncStreamReader<MessageRequest> requestStream,
        ServerCallContext context)
    {
        await foreach (var req in requestStream.ReadAllAsync(context.CancellationToken))
        {
            var nameClaim = context.GetHttpContext().User.Claims.SingleOrDefault(x => x.Type == ClaimTypes.Name);
            if (nameClaim is null)
                throw new RpcException(new Status(StatusCode.Unauthenticated, "Failed to get user name"));
           
            var message = Message.From(req, nameClaim.Value);
            
            RwLock.EnterWriteLock();
            try
            {
                History.Add(message);
            }
            finally
            {
                RwLock.ExitWriteLock();
            }
            
            await BroadcastMessage(message.ToResponse(), context);
        }
    }
    
    private static async Task BroadcastMessage(
        MessageResponse message,
        ServerCallContext context)
    {
        foreach (var (_, responseStream) in Clients)
        {
            await responseStream.WriteAsync(message, context.CancellationToken);
        }
    }
}