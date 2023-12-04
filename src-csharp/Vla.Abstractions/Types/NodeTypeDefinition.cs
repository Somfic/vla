using System.Collections.Immutable;
using System.Drawing;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Vla.Abstractions.Types;

public readonly struct NodeTypeDefinition
{
    public NodeTypeDefinition(Type type, string? name = "")
    {
        Type = type;
        Name = string.IsNullOrWhiteSpace(name) ? type.Name : name;
        Color = TypeToColor(type);

        if (Name.EndsWith('&'))
            Name = Name[..^1];
        
        if (type.IsEnum)
            Values = Enum.GetValues(type).Cast<object>().Select(x => new NodeTypeDefinitionValue(EnumExtensions.GetValueNameFromField(x.GetType(), x.ToString()), x)).ToImmutableArray();

        if(type.IsEnum)
            HtmlType = "select";
        else
            HtmlType = type.Name.Replace("&", "") switch
            {
                "Int32" => "number",
                "Double" => "number",
                "String" => "text",
                "Boolean" => "checkbox",
                _ => "???"
            };
        
        Shape = type == typeof(NodeFlow) ? "diamond" : "circle";
        DefaultValue = GetDefaultValueForType(type);
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

    private static object? GetDefaultValueForType(Type type)
    {
        if (type == typeof(string))
            return string.Empty;

        if (type.Name.EndsWith('&'))
            return string.Empty;
        
        Console.WriteLine(type.FullName);

        return type.GetDefaultValueForType();
    }

    private static Color TypeToColor(Type type)
    {
        var typeColors = new Dictionary<Type, Color>
        {
            { typeof(int), Color.FromArgb(139, 255, 109) },
            { typeof(double), Color.FromArgb(255, 223, 109) },
            { typeof(string), Color.FromArgb(109, 159, 255) },
            { typeof(bool), Color.FromArgb(255, 109, 109) },
            { typeof(NodeFlow), Color.FromArgb(255, 255, 255) },
        };

        return typeColors.TryGetValue(type, out var color) ? color : Color.FromArgb(217, 109, 255);
    }
}

public readonly struct NodeTypeDefinitionValue(string name, object? value)
{
    [JsonProperty("name")]
    public string Name { get; init; } = name;

    [JsonProperty("value")]
    public object? Value { get; init; } = value;
}