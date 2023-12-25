using System.Collections.Immutable;
using Newtonsoft.Json;
using Vla.Abstractions.Instance;

namespace Vla.Server.Messages;

public readonly struct RunWorkspaceMessage(Abstractions.Web.Workspace workspace) : ISocketMessage
{
    [JsonProperty("workspace")]
    public Abstractions.Web.Workspace Workspace { get; init; } = workspace;
}

public readonly struct UpdateWebMessage(Abstractions.Web.Web web) : ISocketMessage
{
    [JsonProperty("web")]
    public Abstractions.Web.Web Web { get; init; } = web;
    
    [JsonProperty("workspace")]
    public string WorkspacePath { get; init; } = string.Empty;
}
public readonly struct ExecutionResultMessage(ImmutableArray<NodeExecutionResult> results) : ISocketMessage
{
    [JsonProperty("results")]
    public ImmutableArray<NodeExecutionResult> Results { get; init; } = results;
}