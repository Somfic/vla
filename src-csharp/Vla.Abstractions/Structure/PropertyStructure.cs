using Newtonsoft.Json;

namespace Vla.Abstractions.Structure;

public readonly struct PropertyStructure
{
    public PropertyStructure(string name, Type type, string defaultValue)
    {
        Name = name;
        Type = type;
        DefaultValue = defaultValue;
        
        if(type.IsEnum)
            HtmlType = "select";
        else
            HtmlType = type.Name switch
        {
            "Int32" => "number",
            "Double" => "number",
            "String" => "text",
            "Boolean" => "checkbox",
            _ => "text"
        };
    }

    [JsonProperty("name")]
    public string Name { get; init; }

    [JsonProperty("type")]
    public Type Type { get; init; }

    [JsonProperty("htmlType")]
    public string HtmlType { get; init; }

    [JsonProperty("defaultValue")]
    public string DefaultValue { get; init; }
}