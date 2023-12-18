using System.Collections.Immutable;
using System.Reflection;
using Newtonsoft.Json;

namespace Vla.Abstractions.Structure;

public static class NodeStructureBuilderExtensions
{
    public static NodeStructure WithName(this NodeStructure node, string name)
    {
        node = node.WithSearchTerms(name);
        return node with { Name = name };
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

    public static NodeStructure WithInput(this NodeStructure node, string id, string name, Type type)
    {
        return node with { Inputs = node.Inputs.Add(new ParameterStructure(id, name, type)) };
    }

    public static NodeStructure WithInputs(this NodeStructure node, params ParameterStructure[] inputs)
    {
        return node with { Inputs = node.Inputs.AddRange(inputs) };
    }

    public static NodeStructure WithOutput(this NodeStructure node, string id, string name, Type type)
    {
        return node with { Outputs = node.Outputs.Add(new ParameterStructure(id, name, type)) };
    }

    public static NodeStructure WithOutputs(this NodeStructure node, params ParameterStructure[] outputs)
    {
        return node with { Outputs = node.Outputs.AddRange(outputs) };
    }

    public static NodeStructure WithProperty<TValue>(this NodeStructure node, string name, Type type, TValue defaultValue)
    {
        return node with { Properties = node.Properties.Add(new PropertyStructure(name, type, JsonConvert.SerializeObject(defaultValue))) };
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