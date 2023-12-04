namespace Vla.Abstractions;

public readonly struct NodeFlow
{
	public bool Triggered { get; init; }
    
	public static NodeFlow Execute()
	{
		return new NodeFlow { Triggered = true };
	}
    
	public static NodeFlow Ignore()
	{
		return new NodeFlow { Triggered = false };
	}
}