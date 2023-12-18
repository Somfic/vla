using System.Collections.Immutable;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Vla.Abstractions.Extensions;

namespace Vla.Extensions.Core;

[NodeExtension("Core", "Vla's core building blocks")]
public class Core(ILogger<Core> log) : Extension
{
    public override ImmutableArray<Dependency> Dependencies => ImmutableArray<Dependency>.Empty;

    public override Task OnStart()
    {
        log.LogInformation("Hello from Core!");
        return Task.CompletedTask;
    }
}