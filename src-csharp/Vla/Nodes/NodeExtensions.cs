using System.Reflection;
using Somfic.Common;
using Vla.Addon;

namespace Vla.Nodes;

public static class NodeExtensions
{
    private static string GetName(Type type)
    {
        return type.Name;
    }
    
    private static NodePurity GetPurity(Type type)
    {
        return type.GetCustomAttribute<NodeAttribute>()?.Purity ?? NodePurity.Deterministic;
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