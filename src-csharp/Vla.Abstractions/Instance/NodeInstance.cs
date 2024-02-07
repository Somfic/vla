using System.Collections.Immutable;
using Newtonsoft.Json;

namespace Vla.Abstractions.Instance;

public readonly struct NodeInstance()
{ 
    [JsonProperty("id")]
    public Guid Id { get; init; } = Guid.NewGuid();

    [JsonProperty("nodeType")]
    public Type NodeType { get; init; } = typeof(object);

    [JsonProperty("properties")]
    public ImmutableArray<PropertyInstance> Properties { get; init; } = ImmutableArray<PropertyInstance>.Empty;

    [JsonProperty("inputs")]
    public ImmutableArray<ParameterInstance> Inputs { get; init; } = ImmutableArray<ParameterInstance>.Empty;
    
    [JsonProperty("metadata")]
    public Metadata Metadata { get; init; } = new();
}

public readonly struct NodeExecutionResult()
{
    [JsonProperty("instanceId")]
    public Guid InstanceId { get; init; } = Guid.Empty;
    
    [JsonProperty("wasExecuted")]
    public bool WasExecuted { get; init; } = false;
    
    [JsonProperty("inputs")]
    public ImmutableArray<ParameterResult> Inputs { get; init; } = ImmutableArray<ParameterResult>.Empty;
    
    [JsonProperty("outputs")]
    public ImmutableArray<ParameterResult> Outputs { get; init; } = ImmutableArray<ParameterResult>.Empty;

    [JsonProperty("exception")]
    public Exception? Exception { get; init; } = null;
}

public readonly struct ParameterResult(string parameterId, object? value)
{
    [JsonProperty("parameterId")]
    public string ParameterId { get; init; } = parameterId;

    [JsonProperty("value")]
    public object? Value { get; init; } = value;
}