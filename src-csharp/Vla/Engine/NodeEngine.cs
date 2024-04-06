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
		var nodes = Instances
			.Where(x => x.Purity == NodePurity.Probabilistic)
			.Where(x => x.IncomingBranches.Count == 0)
			.Where(x => x.OutgoingBranches.Count > 0)
			.ToArray();

		if (nodes.Length == 0)
			_log.LogWarning("No entry points were found. Could not find probabilistic nodes with no incoming branches and one or more outgoing branches. This tick will have no effect");

		var results = ImmutableArray.CreateBuilder<NodeExecutionResult>();
		
		foreach (var node in nodes)
			results.AddRange(await ExecuteNode(node));

		return results.ToImmutableArray();
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
			// Check 
			
			
			await node.Execute();
			
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