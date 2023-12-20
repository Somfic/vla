namespace Vla.Helpers;

public static class TypeExtensions
{
	public static object? GetDefaultValueForType(this Type type)
	{
		if (type == typeof(string))
			return string.Empty;

		// If the type is a reference type, return the default value of the base type
		if (type.Name.EndsWith('&'))
		{
			var baseTypeName = (type.FullName ?? type.Name).Replace("&", string.Empty);
			var baseType = type.Assembly.GetType(baseTypeName);
			return baseType?.GetDefaultValueForType();
		}

		if (type.IsValueType)
			return Activator.CreateInstance(type);

		return default;
	}
}