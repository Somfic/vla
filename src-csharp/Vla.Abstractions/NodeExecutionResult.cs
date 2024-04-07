using System.Collections.Immutable;
using Vla.Addon;

namespace Vla.Abstractions;

public readonly struct NodeExecutionResult
{
	public Guid Id { get; }

	public bool Executed { get;init; }
	
	public string Name { get; }

	public ImmutableArray<NodeInput> Inputs { get; } = ImmutableArray<NodeInput>.Empty;

	public ImmutableArray<NodeOutput> Outputs { get; } = ImmutableArray<NodeOutput>.Empty;

	public Exception? Exception { get; } = null;

	public NodeExecutionResult(string name, ImmutableArray<NodeInput> inputs, ImmutableArray<NodeOutput> outputs, Guid id,
		bool executed)
	{
		Name = name;
		Inputs = inputs;
		Outputs = outputs;
		Exception = null;
		Id = id;
		Executed = executed;
	}

	public NodeExecutionResult(string name, Exception exception, Guid id, bool executed)
	{
		Name = name;
		Exception = exception;
		Outputs = ImmutableArray<NodeOutput>.Empty;
		Id = id;
		Executed = executed;
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