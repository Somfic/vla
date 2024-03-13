using System.Collections.Immutable;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Somfic.Common;
using Vla.Server.Messages;
using Vla.Server.Messages.Requests;
using Vla.Server.Messages.Response;
using Vla.Websocket;
using Vla.Workspace;
using WatsonWebsocket;

namespace Vla.Server;

public class ServerService
{
	private readonly ILogger<ServerService> _log;
	private readonly IWebsocketService _server;
	private readonly WorkspaceService _workspaces;

	private ImmutableArray<Func<Task>> _tickCallbacks = ImmutableArray<Func<Task>>.Empty;

	public ServerService(ILogger<ServerService> log, IWebsocketService server, WorkspaceService workspaces)
	{
		_log = log;
		_server = server;
		_workspaces = workspaces;

		_server.ClientConnected.OnChange(async client =>
		{
			try
			{
				await OnNewClient(client);
			}
			catch (Exception ex)
			{
				ex.Data.Add("Client", client);
				_log.LogWarning(ex, "Could not process newly connecting client");
			}
		});

		_server.MessageReceived.OnChange(async incoming =>
		{
			var (client, message) = incoming;

			try
			{
				await OnIncomingMessage(client, message);
			}
			catch (Exception ex)
			{
				ex.Data.Add("Client", client);
				ex.Data.Add("Message", message);
				_log.LogWarning(ex, "Could not process incoming message");
			}
		});
	}

	public async Task StartAsync()
	{
		await _server.StartAsync();

		while (_server.IsRunning)
		{
			foreach (var callback in _tickCallbacks) await callback();

			await Task.Delay(100);
		}
	}

	public Task StopAsync()
	{
		return _server.StopAsync();
	}

	public void OnTick(Func<Task> callback)
	{
		_tickCallbacks = _tickCallbacks.Add(callback);
	}

	private async Task OnIncomingMessage(ClientMetadata client, string message)
	{
		var jObject = JObject.Parse(message);
		var id = jObject["id"]?.Value<string>()?.ToLower() ?? string.Empty;

		if (string.IsNullOrEmpty(id))
		{
			_log.LogWarning("Incoming request did not have an ID, skipping");
			return;
		}

		// TODO: Cache the available methods, since they're constant at runtime
		var method = GetType()
			.GetMethods()
			.Select(x => (method: x, attribute: x.GetCustomAttribute(typeof(RequestAttribute))))
			.Where(x => x.attribute != null)
			.Select(x => (x.method, attribute: x.attribute as RequestAttribute))
			.FirstOrDefault(x => x.attribute!.Id.Equals(id, StringComparison.CurrentCultureIgnoreCase));

		if (method.method == null)
		{
			_log.LogWarning("Could not find a request handler with an implementation for '{Id}, skipping'", id);
			return;
		}

		var methodParameters = method.method.GetParameters();
		var invokingParameters = new dynamic[methodParameters.Length];

		for (var index = 0; index < methodParameters.Length; index++)
		{
			var methodParameter = methodParameters[index];

			if (methodParameter.ParameterType == typeof(ClientMetadata))
				invokingParameters[index] = client;

			if (methodParameter.ParameterType.IsSubclassOf(typeof(ISocketRequest)))
				try
				{
					var request = JsonConvert.DeserializeObject(message, methodParameter.ParameterType);

					if (request == null)
					{
						_log.LogWarning("Could not convert request body to type {Type}", methodParameter.ParameterType);
						return;
					}

					invokingParameters[index] = request;
				}
				catch (Exception ex)
				{
					_log.LogWarning(ex, "Could not convert request body to type {Type}", methodParameter.ParameterType);
					return;
				}
		}

		// Check if the method is async by checking if the return type is Task or Task<T>
		if (method.method.ReturnType == typeof(Task))
		{
			await (Task)method.method.Invoke(this, invokingParameters)!;
		}
		// Check if the method is async and returns an ISocketMessage by checking if the return type is Task<ISocketMessage>, or Task<(type that inherits from ISocketMessage)>
		else if (method.method.ReturnType.IsGenericType &&
		         method.method.ReturnType.GetGenericTypeDefinition() == typeof(Task<>) &&
		         method.method.ReturnType.GetGenericArguments().Length == 1 && method.method.ReturnType
			         .GetGenericArguments().First().IsAssignableTo(typeof(ISocketMessage)))
		{
			var result = await (Task<ISocketMessage>)method.method.Invoke(this, invokingParameters)!;
			await _server.SendAsync(client, result);
		}
		// Else, just invoke the method and ignore the result
		else
		{
			method.method.Invoke(this, invokingParameters);
		}
	}

	private Task OnNewClient(ClientMetadata client)
	{
		return Task.CompletedTask;
	}

	[Request("workspaces-list-recent")]
	private async Task<ISocketMessage> OnWorkspacesListRecent(ClientMetadata client)
	{
		var workspaces = await _workspaces.ListRecentAsync();

		return workspaces
			.Map<ISocketMessage, ImmutableArray<Abstractions.Workspace>>(
				w => new WorkspacesResponse(w),
				e => new ExceptionResponse(e));
	}

	[Request("workspace-create")]
	private async Task<ISocketMessage> OnWorkspaceCreate(ClientMetadata client, CreateWorkspaceRequest request)
	{
		var workspace = await _workspaces.CreateOrLoadAsync(request.Name, request.Path);

		return workspace
			.Map<ISocketMessage, Abstractions.Workspace>(
				w => new WorkspaceResponse(w),
				e => new ExceptionResponse(e));
	}

	[AttributeUsage(AttributeTargets.Method)]
	private class RequestAttribute(string id) : Attribute
	{
		public string Id { get; } = id;
	}
}