using Somfic.Common;
using Vla.Server.Messages;
using WatsonWebsocket;

namespace Vla.Websocket;

public interface IWebsocketService
{
	bool IsRunning { get; }
	AsyncCallback<ClientMetadata> ClientConnected { get; }
	AsyncCallback<(ClientMetadata, string)> MessageReceived { get; }
	Task StartAsync();
	Task StopAsync();
	Task BroadcastAsync<TMessage>(TMessage message) where TMessage : ISocketMessage;
	Task SendAsync<TMessage>(ClientMetadata client, TMessage message) where TMessage : ISocketMessage;
}