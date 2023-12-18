using System.Collections.Immutable;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Vla.Abstractions.Extensions;

namespace Vla.Nodes;

public class ExtensionsService
{
    private readonly ILogger<ExtensionsService> _log;
    private readonly IServiceProvider _services;

    public static readonly string Path = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "Vla", "Extensions");

    public ImmutableDictionary<Manifest, Extension> Extensions { get; private set; } = ImmutableDictionary<Manifest, Extension>.Empty;

    public ExtensionsService(ILogger<ExtensionsService> log, IServiceProvider services)
    {
        _log = log;
        _services = services;
        Directory.CreateDirectory(Path);
    }

    public void RegisterExtensions()
    {
        foreach (var extension in _services.GetServices<Extension>())
        {
            var attribute = extension.GetType().GetCustomAttribute<NodeExtensionAttribute>()!;
            var manifest = new Manifest(attribute.Name, attribute.Description, extension.GetType().Assembly.GetName().Version ?? new Version(0, 0, 0, 0));

            Extensions = Extensions.Add(manifest, extension);
        }
    }

    public async Task OnStart()
    {
        foreach (var extension in Extensions.Values)
        {
            await extension.OnStart();
        }
    }

    public async Task OnStop()
    {
        foreach (var extension in Extensions.Values)
        {
            await extension.OnStop();
        }
    }
}

public static class ExtensionsServiceExtensions
{
    public static IServiceCollection UseExtensions(this IServiceCollection services, string directory)
    {
        var folders = Directory.GetDirectories(directory);

        foreach (var folder in folders)
        {
            var dlls = Directory.GetFiles(folder, "*.dll");

            foreach (var dll in dlls)
            {
                var assembly = Assembly.LoadFrom(dll);
                services = UseExtensions(assembly, services);
            }
        }

        return services;
    }

    public static IServiceCollection UseExtensions(Assembly assembly, IServiceCollection services)
    {
        var extensionTypes = assembly.GetTypes().Where(x => x.IsAssignableTo(typeof(Extension)) && x.GetCustomAttribute<NodeExtensionAttribute>() != null);

        foreach (var extensionType in extensionTypes)
        {
            services = UseExtension(extensionType, services);
        }

        return services;
    }

    public static IServiceCollection UseExtension(Type type, IServiceCollection services)
    {
        if (type.GetCustomAttribute<NodeExtensionAttribute>() == null)
            throw new ArgumentException($"{type.FullName} does not have the {nameof(NodeExtensionAttribute)} attribute");

        if (!type.IsAssignableTo(typeof(Extension)))
            throw new ArgumentException($"{type.FullName} does not inherit from {nameof(Extension)}");

        services.AddSingleton(typeof(Extension), type);

        return services;
    }
}