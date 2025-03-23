using System.Collections.Concurrent;
using System.IdentityModel.Tokens.Jwt;
using Chat.Service.Models;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;

namespace Chat.Service.Services;

public class ChatServiceV4(
    ILogger<ChatServiceV4> logger) : Service.ChatService.ChatServiceBase
{
    private static readonly ConcurrentDictionary<string, IServerStreamWriter<MessageResponse>> Clients = new();
    private static readonly List<Message> History = [];
    private static readonly ReaderWriterLockSlim RwLock = new();

    [Authorize]
    public override async Task<Empty> SendMessage(MessageRequest request, ServerCallContext context)
    {
        var nameClaim = context.GetHttpContext().User.Claims.SingleOrDefault(x => x.Type == JwtRegisteredClaimNames.Name);
        if (nameClaim is null)
            throw new RpcException(new Status(StatusCode.Unauthenticated, "Failed to get user name"));

        var message = Message.From(request, nameClaim.Value);

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
        return new Empty();
    }

    [Authorize]
    public override async Task GetMessages(
        Empty request, 
        IServerStreamWriter<MessageResponse> responseStream,
        ServerCallContext context)
    {
        var clientId = context.Peer;
        if (!Clients.TryAdd(clientId, responseStream))
            logger.LogInformation("Failed to add client {ClientId}", clientId);
            // throw new RpcException(new Status(StatusCode.Cancelled, "Failed to add client"));
        
        await LoadHistory(responseStream, context); 
        
        // костыль?
        context.CancellationToken.Register(() => Clients.TryRemove(clientId, out _));
        while (true)
        {
            await Task.Delay(1000, context.CancellationToken);
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