using System.Collections.Immutable;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Vla.Addon;

namespace Vla.Engine;

public class NodeEngine
{
	private readonly IServiceProvider _serviceProvider;
	private readonly ILogger<NodeEngine> _log;
	public NodeEngine(ILogger<NodeEngine> log, IServiceProvider serviceProvider)
	{
		_serviceProvider = serviceProvider;
		_log = log;	
	}
	
	public ImmutableArray<Node> Instances { get; private set; } = ImmutableArray<Node>.Empty;
	
	public T CreateInstance<T>(InstanceOptions? options = null) where T : Node
	{
		options ??= new InstanceOptions();
		
		options = options with { Type = typeof(T) };
		
		return (T)CreateInstance(options);
	}
	
	private Node CreateInstance(InstanceOptions options)
	{
		// Check if the type is a node
		if (!options.Type.IsSubclassOf(typeof(Node)))
		{
			_log.LogWarning("Type {Type} is not a node", options.Type.Name);
			throw new InvalidOperationException();
		}
		
		var instance = ActivatorUtilities.CreateInstance(_serviceProvider, options.Type);

		Console.WriteLine(JsonConvert.SerializeObject(options));
		
		foreach (var property in options.Properties)
		{
			var propertyInfo = options.Type.GetProperty(property.Key, BindingFlags.Public | BindingFlags.Instance);
			if (propertyInfo == null)
			{
				_log.LogWarning("Property {Property} not found on node {Node}", property.Key, options.Type.Name);
				continue;
			}
			
			propertyInfo.SetValue(instance, property.Value);
		}

		var nodeInstance = instance as Node ?? throw new InvalidOperationException("Could not create node instance");
		
		Instances = Instances.Add(nodeInstance);

		return nodeInstance;
	}
	
	public async Task<ImmutableArray<NodeExecutionResult>> Tick()
	{
		var results = new List<NodeExecutionResult>();
		
		foreach (var instance in Instances)
		{
			results.Add(await ExecuteNode(instance));
		}
		
		return results.ToImmutableArray();
	}

	private async Task<NodeExecutionResult> ExecuteNode(Node node)
	{
		try
		{
			return new NodeExecutionResult(await node.Execute(), node.Id);
		}
		catch (Exception ex)
		{
			return new NodeExecutionResult(ex, node.Id);
		}
	}

	public void RegisterNodes(Type[] nodes)
	{
		
	}
}

public readonly struct NodeExecutionResult
{
	public Guid Id { get; }
	
	public ImmutableArray<NodeOutput> Outputs { get; }
	
	public Exception? Exception { get; }

	public NodeExecutionResult(ImmutableArray<NodeOutput> outputs, Guid id)
	{
		Outputs = outputs;
		Exception = null;
		Id = id;
	}
	
	public NodeExecutionResult(Exception exception, Guid id)
	{
		Exception = exception;
		Outputs = ImmutableArray<NodeOutput>.Empty;
		Id = Id;
	}
}

public record InstanceOptions
{
	public InstanceOptions()
	{
		Properties = ImmutableDictionary<string, dynamic?>.Empty;
		Id = Guid.NewGuid();
	}

	public ImmutableDictionary<string, dynamic?> Properties { get; init; }
	public Type Type { get; init; }
	public Guid Id { get; init; } 

}