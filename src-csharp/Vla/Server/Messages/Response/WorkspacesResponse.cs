using System.Collections.Immutable;
using Newtonsoft.Json;
using Vla.Abstractions;

namespace Vla.Server.Messages.Response;

public readonly struct WorkspacesResponse(ImmutableArray<Abstractions.Workspace> workspaces) : ISocketResponse
{
	[JsonProperty("workspaces")]
	public ImmutableArray<Abstractions.Workspace> Workspaces { get; init; } = workspaces;
}

public readonly struct TypeDefinitionResponse(TypeDefinition definition) : ISocketResponse
{
    [JsonProperty("definition")]
    public TypeDefinition Definition { get; init; } = definition;
}