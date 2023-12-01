using Newtonsoft.Json;

namespace Vla.Nodes.Web.Result;

public readonly struct ParameterValue
{
    public ParameterValue(string id, string value)
    {
        Id = id;
        Value = value;
    }

    [JsonProperty("id")]
    public string Id { get; init; }

    [JsonProperty("value")]
    public string Value { get; init; }
}