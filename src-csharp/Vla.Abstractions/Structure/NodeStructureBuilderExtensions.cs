using System.Collections.Immutable;
using System.Reflection;

namespace Vla.Abstractions.Structure;

public static class NodeStructureBuilderExtensions
{
    public static NodeStructure WithName(this NodeStructure node, string name)
    {
        node = node.WithSearchTerms(name);
        return node with { Name = name };
    }

    public static NodeStructure WithDescription(this NodeStructure node, string description)
    {
        return node with { Description = description };
    }
    
    public static NodeStructure WithType(this NodeStructure node, Type type)
    {
        node = node.WithSearchTerms(type.Name);
        node = node.WithSearchTerms(type.Namespace?.Split('.') ?? Array.Empty<string>());
        return node with { NodeType = type };
    }

    public static NodeStructure WithSearchTerms(this NodeStructure node, params string[] terms)
    {
        return node with { SearchTerms = node.SearchTerms.AddRange(terms).Distinct().ToImmutableArray() };
    }

    public static NodeStructure WithCategory(this NodeStructure node, string? category)
    {
        node = node.WithSearchTerms(category ?? string.Empty);
        return node with { Category = category };
    }

    public static NodeStructure WithInputs(this NodeStructure node, params InputParameterStructure[] inputs)
    {
        return node with { Inputs = node.Inputs.AddRange(inputs) };
    }

    public static NodeStructure WithOutputs(this NodeStructure node, params OutputParameterStructure[] outputs)
    {
        return node with { Outputs = node.Outputs.AddRange(outputs) };
    }
    
    public static NodeStructure WithProperties(this NodeStructure node, params PropertyStructure[] properties)
    {
        return node with { Properties = node.Properties.AddRange(properties) };
    }

    public static NodeStructure WithMethod(this NodeStructure node, MethodInfo method)
    {
        return node with { ExecuteMethod = method.Name };
    }
}