using System;

namespace Vla.Nodes.Structure;

public readonly struct ParameterStructure
{
    public ParameterStructure(string id, string name, Type type)
    {
        Id = id;
        Name = name;
        Type = type;
    }

    public string Id { get; init; }
	
    public string Name { get; init; }
	
    public Type Type { get; init; }
}