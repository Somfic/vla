using System.Drawing;
using Newtonsoft.Json;
using Vla.Abstractions.Types;

namespace Vla.Abstractions.Instance;

public readonly struct Metadata
{
    public Metadata()
    {
    }

    [JsonProperty("position")]
    public Position Position { get; init; } = new();

    [JsonProperty("color")]
    public NodeTypeDefinition.ColorDefinition Color { get; init; } = System.Drawing.Color.White;
}