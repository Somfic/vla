using System;

namespace Vla.Nodes.Attributes;

[AttributeUsage(AttributeTargets.Parameter)]
public class NodeOutputAttribute : Attribute
{
    public NodeOutputAttribute(string name = "")
    {
        Name = name;
    }
	
    public string Name { get; }
}