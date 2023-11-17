using System.Collections.Immutable;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Somfic.Common;
using Vla.Nodes.Connection;
using Vla.Nodes.Instance;
using Vla.Nodes.Structure;
using Vla.Nodes.Types;
using Vla.Nodes.Web;

namespace Vla.Nodes;

public class NodeService
{
    private readonly ILogger<NodeService> _log;
    private readonly List<NodeStructure> _structures = new();

    public NodeService(ILogger<NodeService> log)
    {
        _log = log;
    }

    public IReadOnlyCollection<NodeStructure> Structures => _structures.ToImmutableArray();

    public NodeService Register<TNode>() where TNode : INode
    {
        NodeExtensions.ToStructure<TNode>()
            .On(_structures.Add)
            .OnError(x => _log.LogWarning(x, "Could not register node {Node}", typeof(TNode).Name));

        return this;
    }

    public void Execute(IEnumerable<NodeInstance> instances, IEnumerable<NodeConnection> connections, IEnumerable<NodeStructure> structures)
    {
        var nodeStructures = structures as NodeStructure[] ?? structures.ToArray();
        
        var web = new Web.Web()
            .WithInstances(instances.ToArray())
            .WithConnections(connections.ToArray())
            .Validate(nodeStructures.ToList())
            .On(x => new WebExecutor().ExecuteWeb(x, nodeStructures.ToList()))
            .OnError(x => _log.LogWarning(x, "Could not execute web"));
    }

    public IReadOnlyCollection<NodeTypeDefinition> GenerateTypeDefinitions()
    {
        var inputTypes = Structures.SelectMany(x => x.Inputs).Select(x => x.Type);
        var outputTypes = Structures.SelectMany(x => x.Outputs).Select(x => x.Type);
        var propertyTypes = Structures.SelectMany(x => x.Properties).Select(x => x.Type);
        
        var allTypes = inputTypes.Concat(outputTypes).Concat(propertyTypes).Distinct();

        return allTypes.Select(type => new NodeTypeDefinition(type)).ToList();
    }
}