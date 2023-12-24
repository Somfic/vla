using System.Reflection;

namespace Vla.Addon;

public static class EnumExtensions
{
    public static string GetValueName<T>(this T value) where T : Enum
    {
        return GetValueNameFromEnum(typeof(T), value.ToString());
    }

    public static string GetValueNameFromEnum(Type enumType, string name)
    {
        var memberInfos = enumType.GetMember(name);
        var enumValueMemberInfo = memberInfos.FirstOrDefault(m => m.DeclaringType == enumType);
        var valueAttributes = enumValueMemberInfo?.GetCustomAttribute(typeof(NodeEnumValueAttribute), false);
        return valueAttributes is NodeEnumValueAttribute nodeEnumValueAttribute ? nodeEnumValueAttribute.Name : name;
    }
}