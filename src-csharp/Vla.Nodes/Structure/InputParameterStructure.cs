using System.Reflection;
using Newtonsoft.Json;
using Vla.Helpers;
using Vla.Nodes.Attributes;

namespace Vla.Nodes.Structure;

public readonly struct InputParameterStructure : IParameterStructure
{
    [JsonProperty("id")]
    public string Id { get; init; } 

    [JsonProperty("name")]
    public string Name { get; init; } 
    
    [JsonProperty("description")]
    public string Description { get; init; } 

    [JsonProperty("type")] public Type Type { get; init; } 

    [JsonProperty("defaultValue")]
    public string DefaultValue { get; init; }
    
    public static InputParameterStructure FromParameterInfo(ParameterInfo parameter)
    {
        return new InputParameterStructure
        {
            Id = parameter.Name!,
            Name = parameter.GetCustomAttribute<NodeInputAttribute>()?.Name ?? parameter.Name!,
            Description = parameter.GetDocumentation(),
            Type = parameter.ParameterType,
            DefaultValue = JsonConvert.SerializeObject(parameter.DefaultValue is DBNull ? parameter.ParameterType.GetDefaultValueForType() : parameter.DefaultValue)
        };
    }
}