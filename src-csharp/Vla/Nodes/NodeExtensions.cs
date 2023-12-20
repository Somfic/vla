using System.Reflection;
using Newtonsoft.Json;
using Somfic.Common;
using Vla.Abstractions;
using Vla.Helpers;
using Vla.Nodes.Attributes;
using Vla.Nodes.Structure;

namespace Vla.Nodes;

public static class NodeExtensions
{
    public static Result<NodeStructure> ToStructure(this Type type) =>
        Result.Value(type)
            .Guard(x => !x.IsAbstract, "Node must not be abstract")
            .Guard(x => x.IsAssignableTo(typeof(INode)), "Node must implement INode")
            .Guard(x => x.GetCustomAttribute<NodeAttribute>() is not null, "Node must have a NodeAttribute")
            .Guard(x => GetMainMethod(x).IsSome,
                "Node must have exactly one method with either one or more NodeOutputAttributes or one or more NodeInputAttributes")
            .Guard(x => GetMainMethod(x).Expect().GetParameters()
                    .Where(y => y.GetCustomAttribute<NodeInputAttribute>() is not null)
                    .All(y => y.GetCustomAttribute<NodeOutputAttribute>() is null),
                "Node must have no parameters with both NodeInputAttribute and NodeOutputAttribute")
            .Guard(x => GetMainMethod(x).Expect().ReturnType == typeof(void), "Node must have void as return type")
            .Guard(x => GetMainMethod(x).Expect().IsPublic, "Node must have a public execution path")
            .Pipe(BuildStructure);

    public static Result<NodeStructure> ToStructure<TNode>() where TNode : INode => ToStructure(typeof(TNode));

    private static Result<NodeStructure> BuildStructure(Type type)
    {
        return Result.Try((() =>
            new NodeStructure()
                .WithType(type)
                .WithName(GetName(type))
                .WithDescription(type.GetDocumentation())
                .WithCategory(GetCategory(type))
                .WithSearchTerms(GetSearchTerms(type).ToArray())
                .WithMethod(GetMainMethod(type).Expect())
                .WithProperties(type.GetProperties()
                    .Where(y => y.GetCustomAttribute<NodePropertyAttribute>() is not null)
                    .Select(y => PropertyStructure.FromPropertyInfo(y, type))
                    .ToArray())
                .WithInputs(GetMainMethod(type).Expect().GetParameters()
                    .Where(y => y.GetCustomAttribute<NodeInputAttribute>() is not null)
                    .Select(InputParameterStructure.FromParameterInfo)
                    .ToArray())
                .WithOutputs(GetMainMethod(type).Expect().GetParameters()
                    .Where(y => y.GetCustomAttribute<NodeOutputAttribute>() is not null)
                    .Select(OutputParameterStructure.FromParameterInfo)
                    .ToArray())));
    }

    private static string GetName(Type type)
    {
        return type.GetCustomAttributes()
            .Where(x => x.GetType() == typeof(NodeAttribute))
            .Select(x => x as NodeAttribute)
            .Select(x => x!.Name)
            .First();
    }

    private static string[] GetSearchTerms(Type type)
    {
        return type.GetCustomAttributes()
            .Where(x => x.GetType() == typeof(NodeTagsAttribute))
            .Select(x => x as NodeTagsAttribute)
            .SelectMany(x => x!.Tags)
            .ToArray();
    }

    private static string? GetCategory(Type type)
    {
        return type.GetCustomAttributes()
            .Where(x => x.GetType() == typeof(NodeCategoryAttribute))
            .Select(x => x as NodeCategoryAttribute)
            .Select(x => x!.Name)
            .FirstOrDefault();
    }

    private static Maybe<MethodInfo> GetMainMethod(Type type) => GetApplicableMethods(type).FirstOrDefault();

    private static MethodInfo[] GetApplicableMethods(Type type)
    {
        return type.GetMethods()
            .Where(x => x.GetParameters().Any(y =>
                y.GetCustomAttribute<NodeInputAttribute>() is not null ||
                y.GetCustomAttribute<NodeOutputAttribute>() is not null))
            .ToArray();
    }
}