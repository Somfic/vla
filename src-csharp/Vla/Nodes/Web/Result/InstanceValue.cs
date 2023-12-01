using Newtonsoft.Json;

namespace Vla.Nodes.Web.Result;

public readonly struct InstanceValue
{
	public InstanceValue(string id, object? value)
	{
		Id = id;
		Value = value;
	}

	[JsonProperty("id")]
	public string Id { get; init; }

	[JsonProperty("value")]
	public object? Value { get; init; }
}