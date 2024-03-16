namespace Vla.Server;

[AttributeUsage(AttributeTargets.Method)]
public class ServerMethodAttribute(string method) : Attribute
{
	public string Method { get; } = method;
}

[AttributeUsage(AttributeTargets.Class)]
public class ServerMethodsAttribute(string scope) : Attribute
{
	public string Scope { get; } = scope;
}