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
            { "Int32", ("square", "number", System.Drawing.Color.FromArgb(255, 223, 109)) },
            { "Double", ("circle", "number", System.Drawing.Color.FromArgb(255, 223, 109)) },
            { "String", ("circle", "text", System.Drawing.Color.FromArgb(109, 159, 255)) },
            { "Boolean", ("circle", "checkbox", System.Drawing.Color.FromArgb(255, 109, 109)) },
            { "NodeFlow", ("diamond", "text", System.Drawing.Color.FromArgb(255, 255, 255)) },
        };
    
    public NodeTypeDefinition(Type type, string? name = "")
    {
        Type = type;
        Name = string.IsNullOrWhiteSpace(name) ? type.Name : name;
        Color = SpecialTypes.TryGetValue(type.Name.Replace("&", ""), out var specialType) ? specialType.color : System.Drawing.Color.FromArgb(255, 255, 255);

        if (type.IsEnum)
            HtmlType = "select";
        else
            HtmlType = SpecialTypes.TryGetValue(type.Name.Replace("&", ""), out specialType) ? specialType.htmlType : "text";
        Shape = SpecialTypes.TryGetValue(type.Name.Replace("&", ""), out specialType) ? specialType.shape : "circle";

        if (Name.EndsWith('&'))
            Name = Name[..^1];

        if (type.IsEnum)
            Values = Enum.GetNames(type)
                .Select(x => (value: x, label: EnumExtensions.GetValueNameFromEnum(type, x)))
                .Select(x => new NodeTypeDefinitionValue(x.label, Enum.Parse(type, x.value)))
                .ToImmutableArray();
        
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
    public ColorDefinition Color { get; init; }

    [JsonProperty("shape")]
    public string Shape { get; init; }
    
    [JsonProperty("defaultValue")]
    public object? DefaultValue { get; init; }
    
    public readonly struct ColorDefinition(Color color)
    {
        [JsonProperty("r")]
        public int R { get; init; } = color.R;
        
        [JsonProperty("g")]
        public int G { get; init; } = color.G;
        
        [JsonProperty("b")]
        public int B { get; init; } = color.B;
        
        [JsonProperty("h")]
        public float H { get; init; } = color.GetHue();
        
        [JsonProperty("s")]
        public float S { get; init; } = color.GetSaturation();
        
        [JsonProperty("l")]
        public float L { get; init; } = color.GetBrightness();
        
        [JsonProperty("hex")]
        public string Hex => $"#{R:X2}{G:X2}{B:X2}";
        
        public static implicit operator ColorDefinition(Color color) => new(color);
    }
}

public readonly struct NodeTypeDefinitionValue(string name, object? value)
{
    [JsonProperty("name")]
    public string Name { get; init; } = name;

    [JsonProperty("value")]
    public object? Value { get; init; } = value;
}