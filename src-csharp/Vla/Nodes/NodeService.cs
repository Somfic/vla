using System.Collections.Immutable;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Somfic.Common;
using Vla.Nodes.Connection;
using Vla.Nodes.Instance;
using Vla.Nodes.Structure;
using Vla.Nodes.Types;
using Vla.Nodes.Web;
using Vla.Nodes.Web.Result;

namespace Vla.Nodes;

public class NodeService
{
    private readonly ILogger<NodeService> _log;
    private readonly IServiceProvider _services;
    private readonly List<NodeStructure> _structures = new();

    public NodeService(ILogger<NodeService> log, IServiceProvider services)
    {
        _log = log;
        _services = services;
    }

    public IReadOnlyCollection<NodeStructure> Structures => _structures.ToImmutableArray();

    public NodeService Register<TNode>() where TNode : INode
    {
        NodeExtensions.ToStructure<TNode>()
            .On(_structures.Add)
            .OnError(x => _log.LogWarning(x, "Could not register node {Node}", typeof(TNode).Name));

        return this;
    }

    public Result<WebResult> Execute(Web.Web web)
    {
        return web
            .Validate(_structures)
            .Pipe(ExecuteWeb)
            .OnError(x => _log.LogWarning(x, "Could not execute web"));
    }

    private Result<WebResult> ExecuteWeb(Web.Web web) =>
        Result.Try(() => {
            var executor = ActivatorUtilities.CreateInstance<WebExecutor>(_services);
            return executor.ExecuteWeb(web, _structures);
        });

    public IReadOnlyCollection<NodeTypeDefinition> GenerateTypeDefinitions()
    {
        var inputTypes = Structures.SelectMany(x => x.Inputs).Select(x => x.Type);
        var outputTypes = Structures.SelectMany(x => x.Outputs).Select(x => x.Type);
        var propertyTypes = Structures.SelectMany(x => x.Properties).Select(x => x.Type);
        
        var allTypes = inputTypes.Concat(outputTypes).Concat(propertyTypes).Distinct();

        return allTypes.Select(type => new NodeTypeDefinition(type)).ToList();
    }
}