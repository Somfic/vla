using System.Collections.Immutable;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Somfic.Common;
using Vla.Abstractions.Structure;
using Vla.Abstractions.Types;
using Vla.Addon;

namespace Vla.Nodes;

public class NodeService
{
    private readonly ILogger<NodeService> _log;
    private readonly IServiceProvider _services;

    public NodeService(ILogger<NodeService> log, IServiceProvider services)
    {
        _log = log;
        _services = services;
    }

    public ImmutableArray<NodeStructure> ExtractStructures(Assembly assembly)
    {
        var structures = new List<NodeStructure>();

        var types = assembly.GetTypes().Where(x => x.IsAssignableTo(typeof(INode))).ToArray();

        foreach (var type in types)
        {
            type.ToStructure()
                .On(structures.Add)
                .OnError(x => _log.LogWarning(x, "Could not register node {Node}", type.Name));
        }

        return structures.ToImmutableArray();
    }
    public ImmutableArray<NodeTypeDefinition> GenerateTypeDefinitions(ImmutableArray<NodeStructure> structures)
    {
        var inputTypes = structures.SelectMany(x => x.Inputs).Select(x => x.Type);
        var outputTypes = structures.SelectMany(x => x.Outputs).Select(x => x.Type);
        var propertyTypes = structures.SelectMany(x => x.Properties).Select(x => x.Type);

        var allTypes = inputTypes.Concat(outputTypes).Concat(propertyTypes).Distinct();

        return allTypes.Select(type => new NodeTypeDefinition(type)).Distinct().ToImmutableArray();
    }
}