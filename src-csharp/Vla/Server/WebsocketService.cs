using System.Net;
using System.Text;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Somfic.Common;
using Vla.Server.Messages;
using WatsonWebsocket;

namespace Vla.Server;

public class WebsocketService
{
    private readonly ILogger<WebsocketService> _log;
    private readonly WatsonWsServer _server;
    private bool _isReady;

    public bool IsRunning => _server.IsListening;

    public AsyncCallback<ClientMetadata> ClientConnected { get; } = new();
    public AsyncCallback<(ClientMetadata, string)> MessageReceived { get; } = new();

    public WebsocketService(ILogger<WebsocketService> log)
    {
        _log = log;
        _server = new WatsonWsServer(IPAddress.Loopback.ToString(), 55155);
        _server.ClientConnected += async (_, e) => await OnClientConnected(e);
        _server.MessageReceived += async (_, e) => await OnMessageReceived(e);
    }

    public async Task StartAsync()
    {
        await _server.StartAsync();
        _log.LogInformation("Websocket server started");
    }

    public async Task BroadcastAsync<TMessage>(TMessage message) where TMessage : ISocketMessage
    {
        foreach (var client in _server.ListClients())
        {
            await SendAsync(client, message);
        }
    }

    public async Task SendAsync<TMessage>(ClientMetadata client, TMessage message) where TMessage : ISocketMessage
    {
        var wrappedMessage = new ServerMessage<TMessage>(message);
        var json = JsonConvert.SerializeObject(wrappedMessage);
        await _server.SendAsync(client.Guid, json);
    }

    private async Task OnClientConnected(ConnectionEventArgs e)
    {
        _log.LogInformation("Client connected: {Guid}", e.Client.Guid);
        await SendAsync(e.Client, new ReadyStateMessage(_isReady));
        await ClientConnected.Set(e.Client);
    }

    private async Task OnMessageReceived(MessageReceivedEventArgs e)
    {
        await MessageReceived.Set((e.Client, Encoding.UTF8.GetString(e.Data.ToArray())));
    }

    public async Task MarkReady(bool state = true)
    {
        _isReady = state;
        await BroadcastAsync(new ReadyStateMessage(_isReady));
    }

    private readonly struct ServerMessage<TMessage>(TMessage message) where TMessage : ISocketMessage
    {
        public static implicit operator ServerMessage<TMessage>(TMessage message) => new(message);

        [JsonProperty("type")]
        public static string Type => typeof(TMessage).Name.Replace("Message", string.Empty);

        [JsonProperty("data")]
        public TMessage Data { get; init; } = message;
    }
}