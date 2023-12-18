using System.Collections.Immutable;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Somfic.Common;
using Vla.Abstractions.Extensions;
using Vla.Abstractions.Structure;
using Vla.Abstractions.Types;
using Vla.Nodes;

namespace Vla.Workspace;

public class WorkspaceService
{
    private readonly ILogger<WorkspaceService> _log;
    private readonly NodeService _nodes;
    private readonly ExtensionsService _extensions;

    private readonly string _path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "Vla", "Workspaces");

    private readonly Abstractions.Web.Web _defaultWeb = new("Untitled web");

    public WorkspaceService(ILogger<WorkspaceService> log, NodeService nodes, ExtensionsService extensions)
    {
        _log = log;
        _nodes = nodes;
        _extensions = extensions;
        Directory.CreateDirectory(_path);
    }

    public async Task<Result<Abstractions.Web.Workspace>> CreateOrLoadAsync(string name)
    {
        if (Exists(name))
            return await LoadAsync(name);
        return await CreateAsync(name);
    }

    public async Task<ImmutableArray<Abstractions.Web.Workspace>> ListAsync()
    {
        var files = Directory.GetFiles(_path, "*.vla");
        var workspaces = new List<Abstractions.Web.Workspace>();

        foreach (var file in files)
        {
            (await LoadAsync(Path.GetFileNameWithoutExtension(file)))
                .On(workspaces.Add);
        }

        return workspaces.ToImmutableArray();
    }

    public async Task SaveAsync(Abstractions.Web.Workspace workspace)
    {
        (await Result.TryAsync(async () =>
        {
            workspace = workspace with { LastModified = DateTime.Now };
            await File.WriteAllTextAsync(GetWorkspacePath(workspace.Name), EncodeWorkspace(workspace));
            return true;
        })).On(x => _log.LogInformation("Saved workspace {Name} at {Path}", workspace.Name, workspace.Path))
            .OnError(x => _log.LogWarning(x, "Could not save workspace {Name} at {Path}", workspace.Name, workspace.Path));

    }

    public void Delete(Abstractions.Web.Workspace workspace)
    {
        var path = GetWorkspacePath(workspace.Name);

        if (!Exists(workspace.Name))
            return;

        File.Delete(path);
        _log.LogInformation("Deleted workspace {Name} at {Path}", workspace.Name, path);
    }

    private async Task<Result<Abstractions.Web.Workspace>> CreateAsync(string name)
    {
        if (Exists(name))
            return new Exception($"Workspace {name} already exists");

        var path = GetWorkspacePath(name);

        var result = (await Result.TryAsync(async () =>
            {
                var workspace = new Abstractions.Web.Workspace(name)
                {
                    Path = path,
                    Created = DateTime.Now,
                    LastModified = DateTime.Now,
                    Extensions = [new Dependency("Core", Version.Parse("1.0.0"))]
                };
                await File.WriteAllTextAsync(path, EncodeWorkspace(workspace));
                return workspace;
            }))
            .On(x => _log.LogInformation("Created workspace {Name} at {Path}", x.Name, path))
            .OnError(x => _log.LogWarning(x, "Could not create workspace {Name} at {Path}", name, path));

        if (!result.IsError)
            return await LoadAsync(name);

        return result;
    }

    private async Task<Result<Abstractions.Web.Workspace>> LoadAsync(string name)
    {
        var path = GetWorkspacePath(name);

        if (!Exists(name))
            return new FileNotFoundException($"Workspace {name} does not exist", path);

        return (await Result.TryAsync(async () =>
        {
            var json = await File.ReadAllTextAsync(path);
            var workspace = DecodeWorkspace(json);

            if (workspace.Webs.Length == 0)
                workspace = workspace with { Webs = ImmutableArray.Create(_defaultWeb) };

            workspace = workspace with { Structures = [], Types = [], Path = path };

            foreach (var dependency in workspace.Extensions)
            {
                var extension = _extensions.Extensions.First(x => x.Key.Name == dependency.Name && x.Key.Version >= dependency.Version).Value;
                var structures = _nodes.ExtractStructures(extension.GetType().Assembly);

                workspace = workspace with { Structures = workspace.Structures.AddRange(structures) };
            }

            workspace = workspace with { Types = _nodes.GenerateTypeDefinitions(workspace.Structures) };

            return workspace;
        }))
            .On(x => _log.LogInformation("Loaded workspace {Name} with {Webs} webs at {Path}", x.Name, x.Webs.Length, path))
            .OnError(x => _log.LogWarning(x, "Could not load workspace {Name} at {Path}", name, path));
    }

    public bool Exists(string name)
    {
        return File.Exists(GetWorkspacePath(name));
    }

    private string GetWorkspacePath(string name) => Path.Combine(_path, $"{name}.vla");
    private static string EncodeWorkspace(Abstractions.Web.Workspace workspace)
    {
        workspace = workspace with { Path = string.Empty, Structures = ImmutableArray<NodeStructure>.Empty, Types = ImmutableArray<NodeTypeDefinition>.Empty };
        return JsonConvert.SerializeObject(workspace, Formatting.Indented);
    }

    private static Abstractions.Web.Workspace DecodeWorkspace(string encoded) => JsonConvert.DeserializeObject<Abstractions.Web.Workspace>(encoded);
}