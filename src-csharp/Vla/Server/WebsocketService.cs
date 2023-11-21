using System.Net;
using System.Text;
using System.Threading.Tasks;
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

    public AsyncCallback<ClientMetadata> ClientConnected { get; } = new();
    public AsyncCallback<(ClientMetadata, string)> MessageReceived { get; } = new();
    
    public WebsocketService(ILogger<WebsocketService> log)
    {
        _log = log;
         _server = new WatsonWsServer(IPAddress.Loopback.ToString(), 55155);
         _server.ClientConnected += async (_, e) => await OnClientConnected(e);
         _server.MessageReceived += async (_, e) => await OnMessageReceived(e);
    }

    public async Task Start()
    {
        await _server.StartAsync();
        _log.LogInformation("Websocket server started");
    }
    
    public async Task Broadcast<TMessage>(TMessage message) where TMessage : SocketMessage
    {
        foreach (var client in _server.ListClients())
        {
            await Send(client, message);
        }
    }
    
    public async Task Send<TMessage>(ClientMetadata client, TMessage message) where TMessage : SocketMessage
    {
        await _server.SendAsync(client.Guid, JsonConvert.SerializeObject(message));
    }
    
    private async Task OnClientConnected(ConnectionEventArgs e)
    {
        _log.LogInformation("Client connected: {Guid}", e.Client.Guid);
        await ClientConnected.Set(e.Client);
    }
    
    private async Task OnMessageReceived(MessageReceivedEventArgs e)
    {
        await MessageReceived.Set((e.Client, Encoding.UTF8.GetString(e.Data.ToArray())));
    }
}

