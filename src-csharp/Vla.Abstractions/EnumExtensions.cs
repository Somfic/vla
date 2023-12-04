using Vla.Abstractions.Attributes;

namespace Vla.Abstractions;

public static class EnumExtensions {
	public static string GetValueName<T>(this T value) where T : Enum
	{
		var enumType = typeof(T);
		var name = Enum.GetName(enumType, value);
		var field = enumType.GetField(name);
		return GetValueNameFromField(field.GetType(), name);
	}

	public static string GetValueNameFromField(Type field, string name)
	{
		var attribute = field.GetCustomAttributes(typeof(NodeEnumValueAttribute), false).FirstOrDefault() as NodeEnumValueAttribute;
		return attribute?.Name ?? name;
	}
}