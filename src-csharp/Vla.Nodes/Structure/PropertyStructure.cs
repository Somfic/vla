using System.ComponentModel;
using System.Reflection;
using Newtonsoft.Json;
using Vla.Helpers;
using Vla.Nodes.Attributes;

namespace Vla.Nodes.Structure;

public readonly struct PropertyStructure
{
    [JsonProperty("id")]
    public string Id { get; init; } 
    
    [JsonProperty("name")]
    public string Name { get; init; } 
    
    [JsonProperty("description")]
    public string Description { get; init; }

    [JsonProperty("type")]
    public Type Type { get; init; }

    /// <summary>
    /// The JSON encoded default value of the property.
    /// </summary>
    [JsonProperty("defaultValue")]
    public string DefaultValue { get; init; }
    
    public static PropertyStructure FromPropertyInfo(PropertyInfo propertyInfo, Type type)
    {
        return new PropertyStructure
        {
            Id = propertyInfo.Name,
            Name = propertyInfo.GetCustomAttribute<NodePropertyAttribute>()?.Name ?? propertyInfo.Name,
            Description = propertyInfo.GetDocumentation(),
            Type = propertyInfo.PropertyType,
            DefaultValue = JsonConvert.SerializeObject(propertyInfo.GetValue(Activator.CreateInstance(type)))
        };
    }
}