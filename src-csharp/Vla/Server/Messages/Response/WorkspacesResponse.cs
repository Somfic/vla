using System.Collections.Immutable;
using Newtonsoft.Json;

namespace Vla.Server.Messages.Response;

public readonly struct WorkspacesResponse(ImmutableArray<Abstractions.Workspace> workspaces) : ISocketResponse
{
	[JsonProperty("workspaces")]
	public ImmutableArray<Abstractions.Workspace> Workspaces { get; init; } = workspaces;
}