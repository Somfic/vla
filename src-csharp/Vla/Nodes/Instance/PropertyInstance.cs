using System.Runtime.Serialization;

namespace Vla.Nodes.Instance;

public readonly struct PropertyInstance
{
    public PropertyInstance(string name, Type type, string value)
    {
        Name = name;
        Type = type;
        Value = value;
        DefaultValue = GetDefaultValueForType(type);
    }

    public string Name { get; init; }
    
    public Type Type { get; init; }
    
    public string Value { get; init; }
    
    public object? DefaultValue { get; init; }

    private static object? GetDefaultValueForType(Type type)
    {
        if(type == typeof(string))
            return string.Empty;

        return type.IsValueType ? Activator.CreateInstance(type) : FormatterServices.GetUninitializedObject(type);
    }
}