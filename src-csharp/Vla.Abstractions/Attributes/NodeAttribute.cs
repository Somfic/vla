namespace Vla.Abstractions.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class NodeAttribute : Attribute
{
    
}

[AttributeUsage(AttributeTargets.Class)]
public class NodeCategoryAttribute : Attribute
{
    public NodeCategoryAttribute(string name)
    {
        Name = name;
    }

    public string Name { get; set; }
}

[AttributeUsage(AttributeTargets.Class)]
public class NodeTagsAttribute : Attribute
{
    public NodeTagsAttribute(params string[] tags)
    {
        Tags = tags;
    }

    public string[] Tags { get; set; }
}