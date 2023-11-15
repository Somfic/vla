using Vla.Nodes.Instance;

namespace Vla.Nodes.Connection;

public readonly struct NodeConnection {
    public InstancedProperty From { get; init; }

    public InstancedProperty To { get; init; }
}