using System.Collections.Immutable;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Somfic.Common;

namespace Vla.Workspace;

public class WorkspaceService
{
	private readonly ILogger<WorkspaceService> _log;

	private readonly string _path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
		"Vla", "Workspaces");

	public WorkspaceService(ILogger<WorkspaceService> log)
	{
		_log = log;
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
		await File.WriteAllTextAsync(GetWorkspacePath(workspace.Name), EncodeWorkspace(workspace));
		_log.LogInformation("Saved workspace {Name} at {Path}", workspace.Name, GetWorkspacePath(workspace.Name));
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
		
		return (await Result.TryAsync(async () =>
		{
			var workspace = new Abstractions.Web.Workspace(name);
			await File.WriteAllTextAsync(path, EncodeWorkspace(workspace));
			return workspace;
		}))
			.On(x => _log.LogInformation("Created workspace {Name} at {Path}", x.Name, path))
			.OnError(x => _log.LogWarning(x, "Could not create workspace {Name} at {Path}", name, path));
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
	private static string EncodeWorkspace(Abstractions.Web.Workspace workspace) => JsonConvert.SerializeObject(workspace, Formatting.Indented);
	private static Abstractions.Web.Workspace DecodeWorkspace(string encoded) => JsonConvert.DeserializeObject<Abstractions.Web.Workspace>(encoded);
}