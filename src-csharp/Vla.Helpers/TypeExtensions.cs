namespace Vla.Helpers;

public static class TypeExtensions
{
	public static object? GetDefaultValueForType(this Type type)
	{
		if (type == typeof(string))
			return string.Empty;

		if (type.Name.EndsWith('&'))
			return null;

		if (type.IsValueType)
			return Activator.CreateInstance(type);

		return default;
	}
}