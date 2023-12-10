using System.Collections.Immutable;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Somfic.Common;
using Vla.Abstractions;
using Vla.Abstractions.Structure;
using Vla.Abstractions.Types;
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

    public NodeService RegisterStructure<TNode>() where TNode : INode
    {
        NodeExtensions.ToStructure<TNode>()
            .On(_structures.Add)
            .OnError(x => _log.LogWarning(x, "Could not register node {Node}", typeof(TNode).Name));

        return this;
    }
    
    public NodeService RegisterStructures(Assembly assembly)
    {
        var types = assembly.GetTypes().Where(x => x.IsAssignableTo(typeof(INode))).ToArray();
        
        foreach (var type in types)
        {
            NodeExtensions.ToStructure(type)
                .On(_structures.Add)
                .OnError(x => _log.LogWarning(x, "Could not register node {Node}", type.Name));
        }

        return this;
    }

    public Result<WebResult> Execute(Abstractions.Web.Web web)
    {
        return web
            .Validate(_structures)
            .Pipe(ExecuteWeb)
            .OnError(x => _log.LogWarning(x, "Could not execute web"));
    }

    private Result<WebResult> ExecuteWeb(Abstractions.Web.Web web) =>
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

        return allTypes.Select(type => new NodeTypeDefinition(type)).Distinct().ToList();
    }
}