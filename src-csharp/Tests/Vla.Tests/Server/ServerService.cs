using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Somfic.Common;
using Vla.Server;
using Vla.Server.Messages;
using Vla.Server.Methods;
using Vla.Websocket;
using WatsonWebsocket;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Vla.Tests.Server;

public class ServerService
{
	[Test]
	public async Task ServerService_Start_StartsWebsocket()
	{
		var (server, websocket) = CreateServerService();

		await server.StartAsync();
		
		Assert.That(websocket.IsRunning, Is.True);
	}
	
	[Test]
	public async Task ServerService_Stop_StopsWebsocket()
	{
		var (server, websocket) = CreateServerService();

		await server.StartAsync();
		await server.StopAsync();
		
		Assert.That(websocket.IsRunning, Is.False);
	}
	
	[Test]
	public async Task ServerService_OnTick_CallsTickCallbacks()
	{
		var (server, _) = CreateServerService();
		var ticked = false;

		server.OnTick(async () => { ticked = true; });

		await server.StartAsync();
		await Task.Delay(100);
		await server.StopAsync();
		
		Assert.That(ticked, Is.True);
	}
	
	[Test]
	public async Task ServerService_Methods_CanInvokeWithAsyncValue()
	{
		var client = new ClientMetadata() { Guid = Guid.NewGuid() };
		await TestServerMethod("test-async-value", client);
		Assert.That(TestMethod.TestAsyncValueCalled, Is.True);
	}
	
	[Test]
	public async Task ServerService_Methods_CanInvokeWithAsyncNoValue()
	{
		var client = new ClientMetadata() { Guid = Guid.NewGuid() };
		await TestServerMethod("test-async-novalue", client);
		Assert.That(TestMethod.TestAsyncNoValueCalled, Is.True);
	}
	
	[Test]
	public async Task ServerService_Methods_CanInvokeWithValue()
	{
		var client = new ClientMetadata() { Guid = Guid.NewGuid() };
		await TestServerMethod("test-value", client);
		Assert.That(TestMethod.TestValueCalled, Is.True);
	}
	
	[Test]
	public async Task ServerService_Methods_CanInvokeWithNoValue()
	{ var client = new ClientMetadata() { Guid = Guid.NewGuid() };
		await TestServerMethod("test-novalue", client);
		Assert.That(TestMethod.TestNoValueCalled, Is.True);
	}
	
	[Test]
	public async Task ServerService_Methods_PopulatesClientMetadata()
	{
		var client = new ClientMetadata() { Guid = Guid.NewGuid() };
		await TestServerMethod("test-client-metadata", client);
		Assert.That(TestMethod.TestClientMetadataCalled, Is.True);
		Assert.That(TestMethod.TestClientMetadataClient, Is.Not.Null);
	}
	
	[Test]
	public async Task ServerService_Methods_PopulatesRequest()
	{
		var client = new ClientMetadata() { Guid = Guid.NewGuid() };
		await TestServerMethod("test-request", client);
		Assert.That(TestMethod.TestRequestCalled, Is.True);
		Assert.That(TestMethod.TestRequestRequest, Is.Not.Null);
	}

	private async Task TestServerMethod(string id, ClientMetadata client)
	{
		var (server, websocket) = CreateServerService();
		
		server.AddMethods<TestMethod>();
		
		await server.StartAsync();
		
		websocket.SimulateClientConnected(client);
		websocket.SimulateMessageReceived(client, JsonConvert.SerializeObject(new RequestMessage(id)));

		await Task.Delay(10);
	}

	class TestMethod : IServerMethods
	{
		public static bool TestAsyncValueCalled { get; private set; }
		
		[Request("test-async-value")]
		public Task<ResponseMessage<int>> TestAsyncValue()
		{
			TestAsyncValueCalled = true;
			return Task.FromResult(new ResponseMessage<int>(42));
		}
		
		 public static bool TestAsyncNoValueCalled { get; private set; }
		
		[Request("test-async-novalue")]
		public Task TestAsyncNoValue()
		{
			TestAsyncNoValueCalled = true;
			return Task.CompletedTask;
		}
		
		public static bool TestValueCalled { get; private set; }
		
		[Request("test-value")]
		public ResponseMessage<int> TestValue()
		{
			TestValueCalled = true;
			return new ResponseMessage<int>(42);
		}
		
		public static bool TestNoValueCalled { get; private set; }
		
		[Request("test-novalue")]
		public void TestNoValue()
		{
			TestNoValueCalled = true;
		}
		
		public static bool TestClientMetadataCalled { get; set; }
		public static ClientMetadata? TestClientMetadataClient { get; set; }
		
		[Request("test-client-metadata")]
		public void TestClientMetadata(ClientMetadata client)
		{
			TestClientMetadataCalled = true;
			TestClientMetadataClient = client;
		}
		
		public static bool TestRequestCalled { get; set; }
		public static RequestMessage? TestRequestRequest { get; set; }
		
		[Request("test-request")]
		public void TestRequest(RequestMessage request)
		{
			TestRequestCalled = true;
			TestRequestRequest = request;
		}
	}


	record ResponseMessage<T> : ISocketResponse
	{
		public ResponseMessage(T value)
		{
			Value = value;
		}
		
		[JsonProperty("value")]
		public T Value { get; init; }
	}
	
	record RequestMessage : ISocketRequest
	{
		public RequestMessage(string Id)
		{
			this.Id = Id;
		}

		[JsonProperty("id")]
		public string Id { get; init; }

		public void Deconstruct(out string Id)
		{
			Id = this.Id;
		}
	}


	private (Vla.Server.ServerService server, MockWebSocketService websocket) CreateServerService()
	{
		var services = Host.CreateDefaultBuilder()
			.ConfigureServices(s => { s.AddSingleton<IWebsocketService, MockWebSocketService>(); })
			.ConfigureLogging(l =>
			{
				l.AddConsole();
				l.SetMinimumLevel(LogLevel.Trace);
			})
			.Build()
			.Services;

		var server = ActivatorUtilities.CreateInstance<Vla.Server.ServerService>(services);
		var websocket = services.GetRequiredService<IWebsocketService>() as MockWebSocketService;
		
		return (server, websocket);
	}
}

internal class MockWebSocketService : IWebsocketService
{
	public readonly Dictionary<ClientMetadata, (List<string> incoming, List<string> outgoing)> Messages = new();
	public bool IsRunning { get; private set; }

	public AsyncCallback<ClientMetadata> ClientConnected { get; } = new();

	public AsyncCallback<(ClientMetadata, string)> MessageReceived { get; } = new();

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
		Messages.Keys.ToList().ForEach(client => SendAsync(client, message));
		return Task.CompletedTask;
	}

	public Task SendAsync<TMessage>(ClientMetadata client, TMessage message) where TMessage : ISocketMessage
	{
		var json = JsonConvert.SerializeObject(message);
		Messages[client].outgoing.Add(json);
		return Task.CompletedTask;
	}

	public void SimulateClientConnected(ClientMetadata client)
	{
		ClientConnected.Set(client);
		Messages.Add(client, (new List<string>(), new List<string>()));
	}

	public void SimulateMessageReceived(ClientMetadata client, string message)
	{
		MessageReceived.Set((client, message));
		Messages[client].incoming.Add(message);
	}
}