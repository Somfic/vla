using System.Reflection;
using Newtonsoft.Json;

namespace Vla.Nodes.Structure;

public static class NodeStructureBuilderExtensions {
    public static NodeStructure WithType(this NodeStructure node, Type type)
    {
        return node with { Type = type };
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
	
    public static NodeStructure WithProperty<TValue>(this NodeStructure node, string name, string type, TValue defaultValue)
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