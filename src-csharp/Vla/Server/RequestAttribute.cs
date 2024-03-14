namespace Vla.Server;

[AttributeUsage(AttributeTargets.Method)]
public class RequestAttribute(string id) : Attribute
{
	public string Id { get; } = id;
}