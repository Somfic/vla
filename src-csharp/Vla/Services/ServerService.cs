using System.Collections.Immutable;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Vla.Server;
using Vla.Server.Messages;
using Vla.Server.Messages.Response;
using Vla.Server.Methods;
using Vla.Websocket;
using WatsonWebsocket;

namespace Vla.Services;

public class ServerService
{
	private readonly ILogger<ServerService> _log;
	private readonly IWebsocketService _server;
	private readonly IServiceProvider _provider;

	private ImmutableArray<IServerMethods> _serverMethods = ImmutableArray<IServerMethods>.Empty;

	public ServerService(ILogger<ServerService> log, IServiceProvider provider, IWebsocketService server)
	{
		_log = log;
		_provider = provider;
		_server = server;
		
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

	public async Task Tick()
	{
		var messages = _server.Poll();

		foreach (var (client, message) in messages)
		{
			await OnIncomingMessage(client, message);
		}
	}

	public Task StartAsync() => _server.StartAsync();

	public Task StopAsync() => _server.StopAsync();

	public void AddMethods(params Type[] methods) => methods.ToList().ForEach(AddMethods);
	
	public void AddMethods<TMethods>() where TMethods : IServerMethods => AddMethods(typeof(TMethods));

	private void AddMethods(Type methods)
	{
		if (ActivatorUtilities.CreateInstance(_provider, methods) is IServerMethods instance)
		{
			_serverMethods = _serverMethods.Add(instance);
		}
		else
		{
			_log.LogWarning("Could not create an instance of '{Type}', skipping", methods);
		}
	}

	private async Task OnIncomingMessage(ClientMetadata client, string message)
	{
		try
		{
			var jObject = JObject.Parse(message);
			var id = jObject["id"]?.Value<string>()?.ToLower() ?? string.Empty;

			if (string.IsNullOrEmpty(id))
			{
				_log.LogWarning("Incoming request did not have an ID, skipping");
				return;
			}
			
			
			// TODO: Maybe cache the available methods, since they're constant at runtime
			var allMethods = _serverMethods.SelectMany(x => x.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance))
				.Select(x => (method: x, attribute: x.GetCustomAttribute(typeof(ServerMethodAttribute))))
				.Where(x => x.attribute != null)
				.Select(x => (x.method, methodId: (x.attribute as ServerMethodAttribute)!.Method))
				.Select(x => (x.method, x.methodId, scopeId: x.method.DeclaringType!.GetCustomAttribute<ServerMethodsAttribute>()?.Scope))
				.Select(x => (id: $"{x.scopeId} {x.methodId}".Trim(), x.method))
				 .ToImmutableArray();
			
			var methods = allMethods
				.Where(x => x.id.Equals(id, StringComparison.CurrentCultureIgnoreCase))
				.Select(x => x.method)
				.ToImmutableArray();

			if (!methods.Any())
			{
				_log.LogWarning("No methods found for ID '{Id}', skipping", id);
				_log.LogDebug("Applicable methods: {Methods}", string.Join(", ", allMethods.Select(x => x.id)));
				return;
			}
            
			foreach (var method in methods)
			{
				await InvokeMethod(_serverMethods.First(x => x.GetType() == method.DeclaringType), method, message, client);
			}
		}
		catch (Exception ex)
		{
			ex.Data.Add("Client", client);
			ex.Data.Add("Message", message);
			_log.LogWarning(ex, "Could not process incoming message");
			await _server.SendAsync(client, new ExceptionResponse(ex));
		}
	}
	
	public Task BroadcastAsync<TMessage>(TMessage message) where TMessage : ISocketMessage
	{ 
		return _server.BroadcastAsync(message);
	}

	private async Task InvokeMethod(object instance, MethodInfo method, string message, ClientMetadata client)
	{
		try
		{
			_log.LogDebug("Invoking method '{Namespace}{Method}' with message '{Message}'",
				method.DeclaringType!.FullName, method.Name, message);

			var methodParameters = method.GetParameters();
			var invokingParameters = new dynamic[methodParameters.Length];

			for (var index = 0; index < methodParameters.Length; index++)
			{
				var methodParameter = methodParameters[index];

				if (methodParameter.ParameterType == typeof(ClientMetadata))
					invokingParameters[index] = client;

				else if (methodParameter.ParameterType.IsAssignableTo(typeof(ISocketRequest)))
					try
					{
						_log.LogDebug("Converting request body to type {Type}", methodParameter.ParameterType);
						_log.LogDebug("Request body: {Body}", message);

						var jObject = JObject.Parse(message);
						var request = jObject["data"]?.ToObject(methodParameter.ParameterType);

						if (request == null)
						{
							_log.LogWarning("Could not convert request body to type {Type}",
								methodParameter.ParameterType);
							return;
						}

						invokingParameters[index] = request;
					}
					catch (Exception ex)
					{
						_log.LogWarning(ex, "Could not convert request body to type {Type}",
							methodParameter.ParameterType);
						return;

					}
				else
				{
					_log.LogWarning("Method '{Method}' has an unexpected parameter type '{Type}', skipping",
						method.Name, methodParameter.ParameterType);
					return;
				}
			}

			// If the method returns nothing, just invoke it
			if (method.ReturnType == typeof(Task))
			{
				await (Task)method.Invoke(instance, invokingParameters)!;
			}
			else if (method.ReturnType == typeof(void))
			{
				method.Invoke(instance, invokingParameters);
			}

			// If the method returns a type that inherits from ISocketResponse, invoke it and send the result
			else if (method.ReturnType.IsGenericType &&
			         method.ReturnType.GetGenericTypeDefinition() == typeof(Task<>) &&
			         method.ReturnType.GetGenericArguments().Length == 1 && method.ReturnType
				         .GetGenericArguments().First().IsAssignableTo(typeof(ISocketMessage)))
			{
				var task = (Task)method.Invoke(instance, invokingParameters)!;
				await task;
				var result = task.GetType().GetProperty("Result")!.GetValue(task);

				if (result is ISocketMessage resultMessage)
					await _server.SendAsync(client, resultMessage);
				else
					_log.LogWarning("Method '{Method}' returned an unexpected type '{Type}', skipping",
						method.Name, method.ReturnType);
			}
			else if (method.ReturnType.IsAssignableTo(typeof(ISocketMessage)))
			{
				var result = (ISocketMessage)method.Invoke(instance, invokingParameters)!;
				await _server.SendAsync(client, result);
			}
			else
			{
				_log.LogWarning("Method '{Method}' returned an unexpected type '{Type}', skipping",
					method.Name, method.ReturnType);
			}
		} 
		catch (Exception ex)
		{
			ex.Data.Add("Client", client);
			ex.Data.Add("Message", message);
			_log.LogError(ex, "Could not process incoming message");
			await _server.SendAsync(client, new ExceptionResponse(ex));
		}
	}

	private Task OnNewClient(ClientMetadata client)
	{
		return Task.CompletedTask;
	}

	public async Task SendToAll(string id)
	{
		foreach (var client in _server.Clients)
		{
			await OnIncomingMessage(client, $"{{'id': '{id}'}}");
		}
	}
}