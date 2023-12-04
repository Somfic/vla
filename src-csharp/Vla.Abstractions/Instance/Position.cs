using Newtonsoft.Json;

namespace Vla.Abstractions.Instance;

public readonly struct Position
{
    [JsonProperty("x")]
    public double X { get; init; }

    [JsonProperty("y")]
    public double Y { get; init; }
}