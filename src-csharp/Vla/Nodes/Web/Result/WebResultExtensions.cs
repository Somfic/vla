using System.Collections.Immutable;
using Newtonsoft.Json;

namespace Vla.Nodes.Web.Result;

public static class WebResultExtensions
{
    public static WebResult WithValues(this WebResult result, Dictionary<string, object?> values)
    {
        return result with { Values = values.Select(x => new ParameterValue(x.Key, JsonConvert.SerializeObject(x.Value))).ToImmutableArray() };
    }
    
    public static WebResult WithInstances(this WebResult result, Dictionary<string, object?> instances)
    {
        return result with { Instances = instances.Select(x => new InstanceValue(x.Key, x.Value)).ToImmutableArray() };
    }
}