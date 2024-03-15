using System.Net;
using System.Text;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Somfic.Common;
using Vla.Server.Messages;
using WatsonWebsocket;

namespace Vla.Websocket;

public class WebsocketService : IWebsocketService
{
	private readonly ILogger<WebsocketService> _log;
	private readonly WatsonWsServer _server;

	public WebsocketService(ILogger<WebsocketService> log)
	{
		_log = log;
		_server = new WatsonWsServer(IPAddress.Loopback.ToString(), 55155);
		_server.ClientConnected += async (_, e) => await OnClientConnected(e);
		_server.MessageReceived += async (_, e) => await OnMessageReceived(e);
	}

	public bool IsRunning => _server.IsListening;

	public AsyncCallback<ClientMetadata> ClientConnected { get; } = new();
	public AsyncCallback<(ClientMetadata, string)> MessageReceived { get; } = new();

	public async Task StartAsync()
	{
		await _server.StartAsync();
		_log.LogInformation("Websocket server started");
	}

	public async Task StopAsync()
	{
		_server.Stop();

		while (_server.IsListening) await Task.Delay(1);
	}

	public async Task BroadcastAsync<TMessage>(TMessage message) where TMessage : class, ISocketMessage
	{
		_log.LogDebug("Starting broadcast of '{Message}'", message);
		foreach (var client in _server.ListClients()) await SendAsync(client, message);
		_log.LogDebug("Broadcast of '{Message}' completed", message);
	}

	public async Task SendAsync<TMessage>(ClientMetadata client, TMessage message) where TMessage : class, ISocketMessage
	{
		var wrappedMessage = new ServerMessage<TMessage>(message);
		var json = JsonConvert.SerializeObject(wrappedMessage);
		_log.LogDebug("Sending message to {Guid}: '{Message}'", client.Guid, json);
		await _server.SendAsync(client.Guid, json);
	}

	private async Task OnClientConnected(ConnectionEventArgs e)
	{
		_log.LogInformation("Client connected: {Guid}", e.Client.Guid);
		await ClientConnected.Set(e.Client);
	}

	private async Task OnMessageReceived(MessageReceivedEventArgs e)
	{
		var message = Encoding.UTF8.GetString(e.Data.ToArray());
		_log.LogDebug("Message received from {Guid}: '{Message}'", e.Client.Guid, message);
		await MessageReceived.Set((e.Client, message));
	}


	private readonly struct ServerMessage<TMessage>(TMessage message)
		where TMessage : ISocketMessage
	{
		// Place a space between camel case

		public static implicit operator ServerMessage<TMessage>(TMessage message)
		{
			return new ServerMessage<TMessage>(message);
		}

		[JsonProperty("id")]
		public string Id { get; init; } = message.GetType().Name
			.Replace("Message", string.Empty)
			.Replace("Request", string.Empty)
			.Replace("Response", string.Empty)
			.Replace("([a-z])([A-Z])", "$1 $2") // Place a space between camel case
			.ToLower();

		[JsonProperty("data")]
		public TMessage Data { get; init; } = message;
	}
}