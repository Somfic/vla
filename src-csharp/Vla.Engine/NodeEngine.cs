using System.Collections.Immutable;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Vla.Abstractions.Connection;
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

	public ImmutableArray<NodeConnection> Connections { get; private set; } = ImmutableArray<NodeConnection>.Empty;

	public void CreateConnection(Node source, string sourceOutput, Node target, string targetInput) => CreateConnection(new NodeConnection(source, sourceOutput, target, targetInput));

	public void CreateConnection(NodeConnection connection)
	{
		Connections = Connections.Add(connection);
	}

	public T CreateInstance<T>(InstanceOptions? options = null) where T : Node
	{
		options ??= new InstanceOptions();

		return (T)CreateInstance(typeof(T), options);
	}

	private Node CreateInstance(Type type, InstanceOptions options)
	{
		// Check if the type is a node
		if (!type.IsSubclassOf(typeof(Node)))
		{
			_log.LogWarning("Type {Type} is not a node", type.Name);
			throw new InvalidOperationException();
		}

		var instance = ActivatorUtilities.CreateInstance(_serviceProvider, type);

		foreach (var property in options.Properties)
		{
			var propertyInfo = type.GetProperty(property.Key, BindingFlags.Public | BindingFlags.Instance);
			if (propertyInfo == null)
			{
				_log.LogWarning("Property {Property} not found on node {Node}", property.Key, type.Name);
				continue;
			}

			propertyInfo.SetValue(instance, property.Value);
		}

		var nodeInstance = instance as Node ?? throw new InvalidOperationException("Could not create node instance");

		nodeInstance.Id = Guid.NewGuid();

		Instances = Instances.Add(nodeInstance);

		return nodeInstance;
	}

	public async Task<ImmutableArray<NodeExecutionResult>> Tick()
	{
		var sorter = new TopologicalSorter(Connections.Select(x => (x.Source.InstanceId.ToString(), x.Target.InstanceId.ToString())).ToArray());

		var sortedInstances = sorter.Sort()
			.Select(x => Guid.Parse(x.value))
			.ToArray();

		var unsortedInstances = Instances.Select(x => x.Id).Except(sortedInstances);
		
		var instances = sortedInstances.Concat(unsortedInstances).ToImmutableArray();

		var results = new List<NodeExecutionResult>();

		foreach (var instanceId in instances)
		{
			var instance = Instances.First(x => x.Id == instanceId);
			
			var result = await ExecuteNode(instance);
			
			// Loop over all the outputs of this node execution
			foreach (var output in result.Outputs)
			{
				Console.WriteLine($"Searching for inputs {instance.Id}.{output.Name} connects to");

				// Get all the inputs this output is connected to
				var inputs = Connections
					.Where(x => x.Source.InstanceId == instance.Id && x.Source.PropertyId == output.Name)
					.Select(x => x.Target);

				Console.WriteLine($"Found {JsonConvert.SerializeObject(inputs)}");

			// For each input, set the value to the original output, with conversion to the input type
				foreach (var input in inputs)
				{
					var node = Instances.First(x => x.Id == input.InstanceId);
					var nodeIndex = Instances.IndexOf(node);

					Instances[nodeIndex].SetInput(input.PropertyId, output.Value);
					
					Console.WriteLine($"Setting input {input.Id} on {node.Name} to {output.Value}");
					
					Instances = Instances.SetItem(nodeIndex, node);
				}
			}

			results.Add(result);
		}

		return results.ToImmutableArray();
	}

	private async Task<NodeExecutionResult> ExecuteNode(Node node)
	{
		try
		{
			Console.WriteLine($"Executing {node.GetType().Name} with {JsonConvert.SerializeObject(node.Inputs)}");
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
	}

	public ImmutableDictionary<string, dynamic?> Properties { get; init; }
}