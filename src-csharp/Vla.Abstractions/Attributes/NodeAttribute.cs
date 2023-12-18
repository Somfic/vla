namespace Vla.Abstractions.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class NodeAttribute(string name) : Attribute
{
    public string Name { get; } = name;
}

[AttributeUsage(AttributeTargets.Class)]
public class NodeCategoryAttribute(string name) : Attribute
{
    public string Name { get; } = name;
}

[AttributeUsage(AttributeTargets.Class)]
public class NodeTagsAttribute(params string[] tags) : Attribute
{
    public string[] Tags { get; } = tags;
}

[AttributeUsage(AttributeTargets.Field)]
public class NodeEnumValueAttribute(string name) : Attribute
{
    public string Name { get; } = name;
}