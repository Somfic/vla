using System.Collections.Immutable;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Somfic.Common;
using Vla.Nodes.Connection;
using Vla.Nodes.Instance;
using Vla.Nodes.Structure;
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

    public void Execute(IEnumerable<NodeInstance> instances, IEnumerable<NodeConnection> connections)
    {
        var web = new Web.Web()
            .WithStructures(_structures.ToArray())
            .WithInstances(instances.ToArray())
            .WithConnections(connections.ToArray())
            .Validate()
            .On(new WebExecutor().ExecuteWeb)
            .OnError(x => _log.LogWarning(x, "Could not execute web"));
    }
}