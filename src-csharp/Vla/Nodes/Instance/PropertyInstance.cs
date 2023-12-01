using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Vla.Nodes.Instance;

public readonly struct PropertyInstance
{
    public PropertyInstance(string name, Type type, string value)
    {
        Name = name;
        Type = type;
        Value = value;
        DefaultValue = GetDefaultValueForType(type);
    }

    [JsonProperty("name")]
    public string Name { get; init; }

    [JsonProperty("type")]
    public Type Type { get; init; }

    [JsonProperty("value")]
    public string Value { get; init; }

    [JsonProperty("defaultValue")]
    public object? DefaultValue { get; init; }

    private static object? GetDefaultValueForType(Type type)
    {
        if (type == typeof(string))
            return string.Empty;

        return type.IsValueType ? Activator.CreateInstance(type) : FormatterServices.GetUninitializedObject(type);
    }
}