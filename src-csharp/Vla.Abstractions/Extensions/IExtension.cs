using System.Collections.Immutable;

namespace Vla.Abstractions.Extensions;

public abstract class Extension
{
    public abstract ImmutableArray<Dependency> Dependencies { get; }

    public virtual Task OnStart()
    {
        return Task.CompletedTask;
    }

    public virtual Task OnStop() => Task.CompletedTask;
}

[AttributeUsage(AttributeTargets.Class)]
public class NodeExtensionAttribute : Attribute
{
    public NodeExtensionAttribute(string name, string description)
    {
        Name = name;
        Description = description;
    }

    public string Description { get; }

    public string Name { get; }
}