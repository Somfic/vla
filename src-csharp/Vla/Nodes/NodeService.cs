using Microsoft.Extensions.Logging;

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
    
}
