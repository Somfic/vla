using System.Collections.Immutable;

namespace Vla.Nodes.Web.Result;

public readonly struct WebResult
{
    public WebResult()
    {
        
    }

    public ImmutableArray<ParameterValue> Values { get; init; } = ImmutableArray<ParameterValue>.Empty;
    
    public ImmutableArray<InstanceValue> Instances { get; init; } = ImmutableArray<InstanceValue>.Empty;
}