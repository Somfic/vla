using System.Reflection;
using Newtonsoft.Json;
using Somfic.Common;
using Vla.Nodes.Attributes;

namespace Vla.Nodes.Structure;

public static class NodeExtensions
{
    public static Result<NodeStructure> ToStructure<TNode>() where TNode : INode =>
        Result.Value(typeof(TNode))
            .Guard(x => x.GetCustomAttribute<NodeAttribute>() is not null, "Node must have a NodeAttribute")
            .Guard(x => GetMainMethod(x).IsSome,
                "Node must have exactly one method with either one or more NodeOutputAttributes or one or more NodeInputAttributes")
            .Guard(x => GetMainMethod(x).Expect().GetParameters()
                    .Where(y => y.GetCustomAttribute<NodeInputAttribute>() is not null)
                    .All(y => y.GetCustomAttribute<NodeOutputAttribute>() is null),
                "Node must have no parameters with both NodeInputAttribute and NodeOutputAttribute")
            .Guard(x => GetMainMethod(x).Expect().ReturnType == typeof(void), "Node must have void as return type")
            .Guard(x => GetMainMethod(x).Expect().IsPublic, "Node must have a public execution path")
            .Guard(x => x.GetConstructor(Type.EmptyTypes) is not null,"Node must have a public constructor with no parameters")
            .Pipe(BuildStructure);

    private static Result<NodeStructure> BuildStructure(Type type)
    {
        var structure = new NodeStructure()
            .WithType(type)
            .WithMethod(GetMainMethod(type).Expect())
            .WithProperties(type.GetProperties()
                .Where(y => y.GetCustomAttribute<NodePropertyAttribute>() is not null)
                .Select(y =>
                    new PropertyStructure(y.Name, y.PropertyType.ToString(), JsonConvert.SerializeObject(y.GetValue(Activator.CreateInstance(type)))))
                .ToArray())
            .WithInputs(GetMainMethod(type).Expect().GetParameters()
                .Where(y => y.GetCustomAttribute<NodeInputAttribute>() is not null)
                .Select(y =>
                    new ParameterStructure(y.Name!, y.GetCustomAttribute<NodeInputAttribute>()!.Name,
                        y.ParameterType))
                .ToArray())
            .WithOutputs(GetMainMethod(type).Expect().GetParameters()
                .Where(y => y.GetCustomAttribute<NodeOutputAttribute>() is not null)
                .Select(y =>
                    new ParameterStructure(y.Name!, y.GetCustomAttribute<NodeOutputAttribute>()!.Name,
                        y.ParameterType))
                .ToArray());
        return structure;
    }
	
    private static Maybe<MethodInfo> GetMainMethod(Type type) => GetApplicableMethods(type).FirstOrDefault();

    private static MethodInfo[] GetApplicableMethods(Type type)
    {
        Console.WriteLine($"Getting applicable methods for {type}");
        Console.WriteLine(JsonConvert.SerializeObject(type.GetMethods().Select(x => x.GetParameters().Select(x => $"{x.Name}: {x.ParameterType}"))));
        
        return type.GetMethods()
            .Where(x => x.GetParameters().Any(y =>
                y.GetCustomAttribute<NodeInputAttribute>() is not null ||
                y.GetCustomAttribute<NodeOutputAttribute>() is not null))
            .ToArray();
    }
}