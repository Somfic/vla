using System.Collections.Immutable;
using Newtonsoft.Json;

namespace Vla.Nodes.Web.Result;

public readonly struct WebResult
{
    public WebResult()
    {

    }

    [JsonProperty("values")]
    public ImmutableArray<ParameterValue> Values { get; init; } = ImmutableArray<ParameterValue>.Empty;

    [JsonProperty("instances")]
    public ImmutableArray<InstanceValue> Instances { get; init; } = ImmutableArray<InstanceValue>.Empty;
}