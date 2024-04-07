using System.Collections.Immutable;
using Somfic.Common;
using Vla.Server.Messages;
using WatsonWebsocket;

namespace Vla.Websocket;

public interface IWebsocketService
{
	ImmutableArray<ClientMetadata> Clients { get; }
	bool IsRunning { get; }
	AsyncCallback<ClientMetadata> ClientConnected { get; }
	AsyncCallback<(ClientMetadata, string)> MessageReceived { get; }
	Task StartAsync();
	Task StopAsync();
	ImmutableList<(ClientMetadata client, string message)> Poll();
	Task BroadcastAsync<TMessage>(TMessage message) where TMessage : ISocketMessage;
	Task SendAsync<TMessage>(ClientMetadata client, TMessage message) where TMessage : ISocketMessage;
}