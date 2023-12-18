using System.Collections.Immutable;
using Newtonsoft.Json;
using Vla.Abstractions.Instance;

namespace Vla.Web.Result;

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