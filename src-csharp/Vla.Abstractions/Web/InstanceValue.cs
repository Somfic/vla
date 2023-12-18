using Newtonsoft.Json;

namespace Vla.Web.Result;

public readonly struct InstanceValue(string id, object? value)
{
    [JsonProperty("id")]
    public string Id { get; init; } = id;

    [JsonProperty("value")]
    public object? Value { get; init; } = value;
}