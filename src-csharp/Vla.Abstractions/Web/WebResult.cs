using System.Collections.Immutable;
using Newtonsoft.Json;
using Vla.Nodes.Instance;

namespace Vla.Abstractions.Web;

public readonly struct WebResult
{
    public WebResult()
    {

    }

    [JsonProperty("values")]
    public ImmutableArray<ParameterInstance> Values { get; init; } = ImmutableArray<ParameterInstance>.Empty;

    [JsonProperty("instances")]
    public ImmutableArray<InstanceValue> Instances { get; init; } = ImmutableArray<InstanceValue>.Empty;
}