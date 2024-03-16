namespace Vla.Addon;

public readonly struct NodeInput(string id, string label, dynamic? value) : INodeValue
{
	public string Id { get; init; } = id;

	public string Label { get; init; } = label;

	public dynamic? Value { get; init; } = value;
}

public readonly struct NodeOutput(string id, string label, dynamic? value) : INodeValue
{
	public string Id { get; init; } = id;

	public string Label { get; init; } = label;

	public dynamic? Value { get; init; } = value;
}

public interface INodeValue
{
	public string Id { get; }

	public string Label { get; }

	public dynamic? Value { get; }
}