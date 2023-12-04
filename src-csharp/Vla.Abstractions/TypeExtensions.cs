using System.Runtime.Serialization;

namespace Vla.Abstractions;

public static class TypeExtensions
{
	public static object? GetDefaultValueForType(this Type type)
	{
		if(type == typeof(string))
			return string.Empty;

		if (type.Name.EndsWith('&'))
			return null;

		return type.IsValueType ? Activator.CreateInstance(type) : FormatterServices.GetUninitializedObject(type);
	}
}