using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using Somfic.Common;
using Vla.Server.Messages;
using Vla.Websocket;
using Vla.Workspace;
using WatsonWebsocket;

namespace Vla.Tests.Server;

public class ServerService
{
	private Vla.Server.ServerService CreateServerService()
	{
		return new Vla.Server.ServerService(
			new LoggerFactory().CreateLogger<Vla.Server.ServerService>(),
			new MockWebSocketService(),
			Mock.Of<WorkspaceService>()
		);
	}
}

internal class MockWebSocketService : IWebsocketService
{
	private readonly Dictionary<ClientMetadata, (List<string> incoming, List<string> outgoing)> _clientMessages = new();
	public bool IsRunning { get; private set; }

	public AsyncCallback<ClientMetadata> ClientConnected { get; }

	public AsyncCallback<(ClientMetadata, string)> MessageReceived { get; }

	public Task StartAsync()
	{
		IsRunning = true;
		return Task.CompletedTask;
	}

	public Task StopAsync()
	{
		IsRunning = false;
		return Task.CompletedTask;
	}

	public Task BroadcastAsync<TMessage>(TMessage message) where TMessage : ISocketMessage
	{
		_clientMessages.Keys.ToList().ForEach(client => SendAsync(client, message));
		return Task.CompletedTask;
	}

	public Task SendAsync<TMessage>(ClientMetadata client, TMessage message) where TMessage : ISocketMessage
	{
		var json = JsonConvert.SerializeObject(message);
		_clientMessages[client].outgoing.Add(json);
		return Task.CompletedTask;
	}

	public void SimulateClientConnected(ClientMetadata client)
	{
		ClientConnected.Set(client);
		_clientMessages.Add(client, (new List<string>(), new List<string>()));
	}

	public void SimulateMessageReceived(ClientMetadata client, string message)
	{
		MessageReceived.Set((client, message));
		_clientMessages[client].incoming.Add(message);
	}
}