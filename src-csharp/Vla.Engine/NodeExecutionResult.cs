using System.Collections.Immutable;
using Vla.Addon;

namespace Vla.Engine;

public readonly struct NodeExecutionResult
{
	public Guid Id { get; }

	public bool Executed { get; }

	public ImmutableArray<NodeOutput> Outputs { get; }

	public Exception? Exception { get; }

	public NodeExecutionResult(ImmutableArray<NodeOutput> outputs, Guid id, bool executed)
	{
		Outputs = outputs;
		Exception = null;
		Id = id;
		Executed = executed;
	}

	public NodeExecutionResult(Exception exception, Guid id, bool executed)
	{
		Exception = exception;
		Outputs = ImmutableArray<NodeOutput>.Empty;
		Id = id;
		Executed = executed;
	}
}