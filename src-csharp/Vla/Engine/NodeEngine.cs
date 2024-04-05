using System.Collections.Immutable;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Vla.Abstractions;
using Vla.Addon;

namespace Vla.Engine;

public class NodeEngine
{
	private readonly ILogger<NodeEngine> _log;
	private readonly IServiceProvider _serviceProvider;

	private Web _web;

	public NodeEngine(ILogger<NodeEngine> log, IServiceProvider serviceProvider)
	{
		_serviceProvider = serviceProvider;
		_log = log;
	}

	public ImmutableArray<Node> Instances { get; private set; } = ImmutableArray<Node>.Empty;

	public ImmutableArray<NodeConnection> Connections { get; private set; } = ImmutableArray<NodeConnection>.Empty;

	public void CreateConnection(Node source, string outputId, Node target, string inputId)
	{
		CreateConnection(new NodeConnection(source.Id.ToString(), outputId, target.Id.ToString(), inputId));
	}

	public void CreateConnection(NodeConnection connection)
	{
		Connections = Connections.Add(connection);
	}

	public T CreateInstance<T>(NodeInstance? options = null) where T : Node
	{
		if (!options.HasValue)
			return (T)CreateInstance(new NodeInstance { Type = typeof(T) });
		return (T)CreateInstance(options.Value with { Type = typeof(T) });
	}

	public Node CreateInstance(NodeInstance options)
	{
		//_log.LogInformation("Creating instance of {Type}", options.Type.Name);
		//_log.LogInformation("{Json}", JsonConvert.SerializeObject(options, Formatting.Indented));

		// Check if the type is a node
		if (!options.Type.IsSubclassOf(typeof(Node)))
		{
			_log.LogWarning("Type {Type} is not a node", options.Type.Name);
			throw new InvalidOperationException($"{options.Type.Name} does not inherit from {nameof(Node)}");
		}

		var nodeInstance = ActivatorUtilities.CreateInstance(_serviceProvider, options.Type) as Node ??
		                   throw new InvalidOperationException("Could not create node instance");

		nodeInstance.Purity = options.Type.GetCustomAttribute<NodeAttribute>()?.Purity ?? NodePurity.Deterministic;
		nodeInstance.Id = options.Guid ?? Guid.NewGuid();
		nodeInstance.Inputs = options.Inputs.ToDictionary(x => x.Id, x => x.Value).ToImmutableDictionary();
		nodeInstance.InputLabels = options.Inputs.ToDictionary(x => x.Id, x => x.Label).ToImmutableDictionary();
		nodeInstance.Outputs = options.Outputs.ToDictionary(x => x.Id, x => x.Value).ToImmutableDictionary();
		nodeInstance.OutputLabels = options.Outputs.ToDictionary(x => x.Id, x => x.Label).ToImmutableDictionary();

		foreach (var property in options.Properties)
		{
			var propertyInfo = options.Type.GetProperty(property.Id, BindingFlags.Public | BindingFlags.Instance);
			if (propertyInfo == null)
			{
				_log.LogWarning("Property {Property} not found on node {Node}", property.Id, options.Type.Name);
				continue;
			}

			var value = property.Value;

			if (propertyInfo.PropertyType.IsEnum)
				value = Convert.ChangeType(value, TypeCode.Int32);
			
			propertyInfo.SetValue(nodeInstance, value);
		}

		Instances = Instances.Add(nodeInstance);

		return nodeInstance;
	}

	public async Task<ImmutableArray<NodeExecutionResult>> Tick()
	{
		var results = new List<NodeExecutionResult>();
		var connections = Connections;
		
		while (true)
		{
			var sorter = new TopologicalSorter(connections
				.Select(x => (x.Source.NodeId.ToString(), x.Target.NodeId.ToString()))
				.ToArray());

			_log.LogDebug("Connections: {Join}",
				string.Join(", ",
					Connections.Select(x =>
						$"{Instances.First(y => y.Id.ToString() == x.Source.NodeId).Name}.{x.Source.Id} -> {Instances.First(y => y.Id.ToString() == x.Target.NodeId).Name}.{x.Target.Id}")));

			var sortedInstances = sorter.Sort()
				.Select(Guid.Parse)
				.ToArray();

			var unsortedInstances = Instances.Select(x => x.Id).Except(sortedInstances);

			var instances = sortedInstances.Concat(unsortedInstances).ToImmutableArray();

			// _log.LogDebug("Execution order: {Join}", string.Join("->", instances.Select(x => Instances.First(y => y.Id == x).Name)));

			foreach (var instanceId in instances)
			{
				var instance = Instances.First(x => x.Id == instanceId);

				_log.LogInformation("Executing node {InstanceName} ({InstanceId})", instance.Name, instance.Id);

				var result = await ExecuteNode(instance);

				if (!result.Executed)
				{
					// Remove any dependants on this node from the 
				}

				_log.LogDebug("Node {InstanceName} ({InstanceId}) executed with result {SerializeObject}",
					instance.Name, instance.Id, JsonConvert.SerializeObject(result, Formatting.Indented));

				// Loop over all the outputs of this node execution
				foreach (var output in result.Outputs)
				{
					_log.LogDebug("Searching for inputs {InstanceId}.{OutputId} connects to", instance.Id, output.Id);

					// Get all the inputs this output is connected to
					var inputs = Connections
						.Where(x => x.Source.NodeId == instance.Id.ToString() && x.Source.Id == output.Id)
						.Select(x => x.Target);

					_log.LogDebug("Found {SerializeObject}", JsonConvert.SerializeObject(inputs));

					// For each input, set the value to the original output, with conversion to the input type
					foreach (var input in inputs)
					{
						var node = Instances.First(x => x.Id.ToString() == input.NodeId);
						var nodeIndex = Instances.IndexOf(node);

						Instances[nodeIndex].SetInput(input.Id, output.Value);

						_log.LogDebug("Setting input {InputId} on {NodeName} to {OutputValue}", input.Id, node.Name,
							(object)output.Value!);

						Instances = Instances.SetItem(nodeIndex, node);
					}
				}

				results.Add(result);
			}
		}
	}

	public Web SaveWeb()
	{
		return _web with
		{
			Instances = Instances.Select<Node, NodeInstance>(x => x).ToImmutableArray(),
			Connections = Connections
		};
	}

	public void LoadWeb(Web web)
	{
		try
		{
			_web = web;

			Instances = _web.Instances.Select(x =>
			{
				var instance = CreateInstance(x);

				foreach (var input in x.Inputs) instance.SetInput(input.Id, input.Value);

				return instance;
			}).ToImmutableArray();

			Connections = _web.Connections;
		} catch (Exception ex)
		{
			_log.LogError(ex, "Could not load web");
		}
	}

	private async Task<NodeExecutionResult> ExecuteNode(Node node)
	{
		try
		{
			Console.WriteLine($"Seeing if {node.Name} ({node.Id}) should be executed");

			// Get all the input branches
			var inputBranches = node
				.Inputs
				.Count(x => x.Value is Branch);

			var hitInputBranches = node
				.Inputs
				.Count(x => x.Value is Branch { Hit: true });

			if (inputBranches != 0)
			{
				Console.WriteLine($"{node.Name} is a branch node with {inputBranches} input branches and {hitInputBranches} hit branches");
			}
			
			if (inputBranches == 0 || hitInputBranches > 0)
			{
				Console.WriteLine($"Executing {node.Name} ({node.Id})");
				await node.Execute();
			}
			
			var inputs = node
				.Inputs
				.Select(x => new NodeInput(x.Key, node.InputLabels.FirstOrDefault(y => y.Key == x.Key).Value ?? x.Key,
					x.Value))
				.ToImmutableArray();

			var outputs = node
				.Outputs
				.Select(x => new NodeOutput(x.Key, node.OutputLabels.FirstOrDefault(y => y.Key == x.Key).Value ?? x.Key,
					x.Value))
				.ToImmutableArray();
			
			return new NodeExecutionResult(node.Name, inputs, outputs, node.Id, inputBranches == 0 || hitInputBranches > 0);
		}
		catch (Exception ex)
		{
			return new NodeExecutionResult(node.Name, ex, node.Id, true);
		}
	}
}