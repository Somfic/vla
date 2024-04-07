using System.Collections.Immutable;
using Newtonsoft.Json;
using Vla.Abstractions;

namespace Vla.Server.Messages.Messages;

public readonly struct ExecutionMessage(ImmutableArray<NodeExecutionResult> results) : ISocketMessage
{
	[JsonProperty("results")]
	public ImmutableArray<NodeExecutionResult> Results { get; init; } = results;
}