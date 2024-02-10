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

	public ImmutableDictionary<string, dynamic?> CachedOutputs { get; private set; } = ImmutableDictionary<string, dynamic?>.Empty;
	
	public void CreateConnection(Node source, string sourceOutput, Node target, string targetInput) => CreateConnection(new NodeConnection(source, sourceOutput, target, targetInput));

	public void CreateConnection(NodeConnection connection)
	{
		Connections = Connections.Add(connection);
	}

	public T CreateInstance<T>(NodeInstance? instance = null) where T : Node
	{
		instance ??= new NodeInstance();

		instance = instance with { Type = typeof(T) };

		return (T)CreateInstance(instance);
	}

	public Node CreateInstance(NodeInstance instance)
	{
		// Check if the type is a node
		if (!instance.Type.IsSubclassOf(typeof(Node)))
		{
			_log.LogWarning("Type {Type} is not a node", instance.Type.Name);
			throw new InvalidOperationException($"{instance.Type.Name} does not inherit from {nameof(Node)}");
		}

		var nodeInstance = ActivatorUtilities.CreateInstance(_serviceProvider, instance.Type) as Node ?? throw new InvalidOperationException("Could not create node instance");

		nodeInstance.Purity = instance.GetType().GetCustomAttribute<NodeAttribute>()?.Purity ?? NodePurity.Deterministic;
		nodeInstance.Id = instance.Guid ?? Guid.NewGuid();
		nodeInstance.Inputs = instance.Inputs;
		nodeInstance.Outputs = instance.Outputs;
		
		foreach (var property in instance.Properties)
		{
			var propertyInfo = instance.Type.GetProperty(property.Key, BindingFlags.Public | BindingFlags.Instance);
			if (propertyInfo == null)
			{
				_log.LogWarning("Property {Property} not found on node {Node}", property.Key, instance.Type.Name);
				continue;
			}

			propertyInfo.SetValue(nodeInstance, property.Value);
		}

		Instances = Instances.Add(nodeInstance);

		return nodeInstance;
	}

	public async Task<ImmutableArray<NodeExecutionResult>> Tick()
	{
		var sorter = new TopologicalSorter(Connections.Select(x => (x.Source.Node.ToString(), x.Target.Node.ToString())).ToArray());

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
					.Where(x => x.Source.Node == instance.Id && x.Source.Name == output.Name)
					.Select(x => x.Target);

				Console.WriteLine($"Found {JsonConvert.SerializeObject(inputs)}");

			    // For each input, set the value to the original output, with conversion to the input type
				foreach (var input in inputs)
				{
					var node = Instances.First(x => x.Id == input.Node);
					var nodeIndex = Instances.IndexOf(node);

					Instances[nodeIndex].SetInput(input.Name, output.Value);
					
					Console.WriteLine($"Setting input {input.Name} on {node.Name} to {output.Value}");
					
					Instances = Instances.SetItem(nodeIndex, node);
				}
			}

			results.Add(result);
		}

		return results.ToImmutableArray();
	}
	
	public string SaveState()
	{
		var state = new EngineState
		{
			Instances = Instances.Select(x => new NodeInstance
			{
				Type = x.GetType(),
				Guid = x.Id,
				Properties = x.Properties,
				Inputs = x.Inputs,
				Outputs = x.Outputs
			}).ToImmutableArray(),
			Connections = Connections
		};

		return JsonConvert.SerializeObject(state, Formatting.Indented);
	}

	public void LoadState(string engineState)
	{
		var state = JsonConvert.DeserializeObject<EngineState>(engineState);
		
		if(state == null)
			throw new InvalidOperationException("Could not deserialize engine state");

		Instances = state.Instances.Select(x =>
		{
			var instance = CreateInstance(x);

			foreach (var input in x.Inputs)
			{
				instance.SetInput(input.Key, input.Value);
			}

			return instance;
		}).ToImmutableArray();

		Connections = state.Connections;
	}

	private async Task<NodeExecutionResult> ExecuteNode(Node node)
	{
		try
		{
			await node.Execute();
			
			var outputs = node
				.Outputs
				.Select(x => new NodeOutput(x.Key, x.Value))
				.ToImmutableArray();
			
			return new NodeExecutionResult(outputs, node.Id, true);
		}
		catch (Exception ex)
		{
			return new NodeExecutionResult(ex, node.Id, true);
		}
	}
}