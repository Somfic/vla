using System.Collections.Immutable;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Vla.Abstractions;

namespace Vla.Nodes;

public class AddonService
{
    private readonly ILogger<AddonService> _log;
    private readonly IServiceProvider _services;
    
    internal static ImmutableArray<Addon.Metadata.Addon> RegisteredAddons = ImmutableArray<Addon.Metadata.Addon>.Empty;
    public ImmutableArray<Addon.Metadata.Addon> Addons => RegisteredAddons;

    public static readonly string Path = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "Vla", "Addons");
    
    public AddonService(ILogger<AddonService> log, IServiceProvider services)
    {
        _log = log;
        _services = services;
        Directory.CreateDirectory(Path);
    }

    public void RegisterAddons()
    {
        _log.LogInformation("Registering addons...");
        foreach (var addon in RegisteredAddons)
        {
            _log.LogInformation("Registering addon {Name} ({Identifier})", addon.Name, addon.Identifier);
        }
    }
}

public static class AddonExtensions
{
    public static IServiceCollection UseAddons(this IServiceCollection services, string directory)
    {
        var dlls = Directory.GetFiles(directory, "*.dll");

        foreach (var dll in dlls)
        {
            var assembly = Assembly.LoadFrom(dll);
            services = UseAddons(assembly, services);
        }
        
        return services;
    }

    public static IServiceCollection UseAddons(Assembly assembly, IServiceCollection services)
    {
        var extensionTypes = assembly.GetTypes().Where(x => x.IsAssignableTo(typeof(Addon.Metadata.Addon)));

        foreach (var extensionType in extensionTypes)
        {
            services = UseAddon(extensionType, services);
        }

        return services;
    }

    private static IServiceCollection UseAddon(Type type, IServiceCollection services)
    {
        if (Activator.CreateInstance(type) is not Addon.Metadata.Addon instance)
            throw new Exception($"Could not create instance of {type.FullName}");

        try
        {
            services = instance.ConfigureServices(services);
            AddonService.RegisteredAddons = AddonService.RegisteredAddons.Add(instance);
        } catch (Exception e)
        {
            throw new Exception($"Could not configure services for {type.FullName}", e);
        }

        return services;
    }
}