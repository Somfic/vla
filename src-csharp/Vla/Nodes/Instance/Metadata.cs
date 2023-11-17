using System.Drawing;

namespace Vla.Nodes.Instance;

public readonly struct Metadata
{
    public Metadata()
    {
    }

    public Position Position { get; init; } = new();
		
    public Color Color { get; init; } = Color.White;
}