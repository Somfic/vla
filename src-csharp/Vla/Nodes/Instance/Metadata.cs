using System.Drawing;
using Newtonsoft.Json;

namespace Vla.Nodes.Instance;

public readonly struct Metadata
{
    public Metadata()
    {
    }

    [JsonProperty("position")]
    public Position Position { get; init; } = new();

    [JsonProperty("color")]
    public Color Color { get; init; } = Color.White;
}