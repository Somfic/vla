using Vla.Nodes.Instance;

namespace Vla.Nodes.Connection;

public readonly struct NodeConnection {
    public ConnectedProperty From { get; init; }

    public ConnectedProperty To { get; init; }
}