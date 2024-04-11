using System.Collections.Immutable;
using Newtonsoft.Json;
using Vla.Addon;

namespace Vla.Abstractions;

public readonly struct NodeExecutionResult
{
	[JsonProperty("nodeId")]
	public Guid NodeId { get; }

	[JsonProperty("hasExecuted")]
	public bool HasExecuted { get;init; }
	
	[JsonProperty("name")]
	public string Name { get; }

	[JsonProperty("inputs")]
	public ImmutableArray<NodeInput> Inputs { get; } = ImmutableArray<NodeInput>.Empty;
	
	[JsonProperty("outputs")]
	public ImmutableArray<NodeOutput> Outputs { get; } = ImmutableArray<NodeOutput>.Empty;

	[JsonProperty("exception")]
	public Exception? Exception { get; } = null;

	public NodeExecutionResult(string name, ImmutableArray<NodeInput> inputs, ImmutableArray<NodeOutput> outputs, Guid nodeId,
		bool hasExecuted)
	{
		Name = name;
		Inputs = inputs;
		Outputs = outputs;
		Exception = null;
		NodeId = nodeId;
		HasExecuted = hasExecuted;
	}

	public NodeExecutionResult(string name, Exception exception, Guid nodeId, bool hasExecuted)
	{
		Name = name;
		Exception = exception;
		Outputs = ImmutableArray<NodeOutput>.Empty;
		NodeId = nodeId;
		HasExecuted = hasExecuted;
	}

	public NodeOutput GetOutput(string id)
	{
		return Outputs.FirstOrDefault(o => o.Id == id);
	}

	public NodeInput GetInput(string id)
	{
		return Inputs.FirstOrDefault(o => o.Id == id);
	}
}