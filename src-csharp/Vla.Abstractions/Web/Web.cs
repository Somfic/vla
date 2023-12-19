using System.Collections.Immutable;
using Newtonsoft.Json;
using Vla.Abstractions.Connection;
using Vla.Nodes.Instance;

namespace Vla.Abstractions.Web;

public readonly struct Web(string name)
{
    /// <summary>
    /// The id of the web.
    /// </summary>
    [JsonProperty("id")]
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>
    /// The name of the web.
    /// </summary>
    [JsonProperty("name")]
    public string Name { get; init; } = name;

    /// <summary>
    /// The node instances contained in the web.
    /// </summary>
    [JsonProperty("instances")]
    public ImmutableArray<NodeInstance> Instances { get; init; } = ImmutableArray<NodeInstance>.Empty;

    /// <summary>
    /// The connections between node instances in the web.
    /// </summary>
    [JsonProperty("connections")]
    public ImmutableArray<NodeConnection> Connections { get; init; } = ImmutableArray<NodeConnection>.Empty;

    /// <summary>
    /// The result of the web. This is automatically set when running.
    /// </summary>
    [JsonProperty("result")]
    public WebResult Result { get; init; } = new();
}