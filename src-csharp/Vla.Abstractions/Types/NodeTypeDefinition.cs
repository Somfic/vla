using System.Collections.Immutable;
using System.Drawing;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Vla.Abstractions.Types;

public readonly struct NodeTypeDefinition
{
    private static readonly Dictionary<string, (string shape, string htmlType, Color color)>
        SpecialTypes = new()
        {
            { "Int32", ("square", "number", Color.FromArgb(255, 223, 109)) },
            { "Double", ("circle", "number", Color.FromArgb(255, 223, 109)) },
            { "String", ("circle", "text", Color.FromArgb(109, 159, 255)) },
            { "Boolean", ("circle", "checkbox", Color.FromArgb(255, 109, 109)) },
            { "NodeFlow", ("diamond", "text", Color.FromArgb(255, 255, 255)) },
        };
    
    public NodeTypeDefinition(Type type, string? name = "")
    {
        Type = type;
        Name = string.IsNullOrWhiteSpace(name) ? type.Name : name;
        Color = SpecialTypes.TryGetValue(type.Name.Replace("&", ""), out var specialType) ? specialType.color : Color.FromArgb(255, 255, 255);

        if (type.IsEnum)
            HtmlType = "select";
        else
            HtmlType = SpecialTypes.TryGetValue(type.Name.Replace("&", ""), out specialType) ? specialType.htmlType : "text";
        Shape = SpecialTypes.TryGetValue(type.Name.Replace("&", ""), out specialType) ? specialType.shape : "circle";

        if (Name.EndsWith('&'))
            Name = Name[..^1];
        
        if (type.IsEnum)
            Values = Enum.GetValues(type).Cast<object>().Select(x => new NodeTypeDefinitionValue(EnumExtensions.GetValueNameFromField(x.GetType(), x.ToString()), x)).ToImmutableArray();
        
        DefaultValue = type.GetDefaultValueForType();
    }

    [JsonProperty("type")]
    public Type Type { get; init; }

    [JsonProperty("name")]
    public string Name { get; init; }
    
    [JsonProperty("htmlType")]
    public string HtmlType { get; init; }
    
    [JsonProperty("values")]
    public ImmutableArray<NodeTypeDefinitionValue> Values { get; init; } = ImmutableArray<NodeTypeDefinitionValue>.Empty;

    [JsonProperty("color")]
    public Color Color { get; init; }

    [JsonProperty("shape")]
    public string Shape { get; init; }
    
    [JsonProperty("defaultValue")]
    public object? DefaultValue { get; init; }
}

public readonly struct NodeTypeDefinitionValue(string name, object? value)
{
    [JsonProperty("name")]
    public string Name { get; init; } = name;

    [JsonProperty("value")]
    public object? Value { get; init; } = value;
}