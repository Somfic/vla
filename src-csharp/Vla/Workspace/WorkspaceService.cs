using System.Collections.Immutable;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Somfic.Common;
using Vla.Abstractions;
using Vla.Addons.Math;
using Vla.Nodes;

namespace Vla.Workspace;

public class WorkspaceService
{
	private static readonly string RecentPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Vla", "recent workspaces");

	private static readonly Abstractions.Web DefaultWeb = new("Pythagorean theorem")
	{
		Instances =
		[
			new NodeInstance
			{
				Guid = Guid.Parse("00000000-0000-0000-0000-000000000001"),
				Type = typeof(MathNode),
				Properties = [new NamedValue("Mode", "Mode", MathNode.MathMode.Power)],
				Inputs = [new NamedValue("power.value", "Value", 3), new NamedValue("power.base", "Base", 2)]
			},
			new NodeInstance
			{
				Guid = Guid.Parse("00000000-0000-0000-0000-000000000002"),
				Type = typeof(MathNode),
				Properties = [new NamedValue("Mode", "Mode", MathNode.MathMode.Power)],
				Inputs = [new NamedValue("power.value", "Value", 4), new NamedValue("power.base", "Base", 2)]
			},
			new NodeInstance
			{
				Guid = Guid.Parse("00000000-0000-0000-0000-000000000003"),
				Type = typeof(MathNode),
				Properties = [new NamedValue("Mode", "Mode", MathNode.MathMode.Add)]
			},
			new NodeInstance
			{
				Guid = Guid.Parse("00000000-0000-0000-0000-000000000004"),
				Type = typeof(MathNode),
				Properties = [new NamedValue("Mode", "Mode", MathNode.MathMode.SquareRoot)]
			}
		],
		Connections =
		[
			new NodeConnection(Guid.Parse("00000000-0000-0000-0000-000000000001"), "power.result",
				Guid.Parse("00000000-0000-0000-0000-000000000003"), "add.a"),
			new NodeConnection(Guid.Parse("00000000-0000-0000-0000-000000000002"), "power.result",
				Guid.Parse("00000000-0000-0000-0000-000000000003"), "add.b"),
			new NodeConnection(Guid.Parse("00000000-0000-0000-0000-000000000003"), "add.result",
				Guid.Parse("00000000-0000-0000-0000-000000000004"), "squareRoot.value")
		]
	};

	private readonly AddonService _addons;

	private readonly ILogger<WorkspaceService> _log;

	public WorkspaceService(ILogger<WorkspaceService> log, AddonService addons)
	{
		_log = log;
		_addons = addons;
	}
	
	public async Task<Result<Abstractions.Workspace>> CreateAsync(string name, string path)
	{
		if (Exists(path))
			return new Exception($"A workspace already exists at {path}");

		var result = (await Result.TryAsync(async () =>
			{
				var workspace = new Abstractions.Workspace(name)
				{
					Name = name,
					Path = path,
					Created = DateTime.Now,
					LastModified = DateTime.Now,
					Addons = [("Core", new Version(0, 0, 0))]
				};
				await File.WriteAllTextAsync(path, EncodeWorkspace(workspace));
				return workspace;
			}))
			.On(x => _log.LogInformation("Created workspace '{Name}' in '{Path}'", x.Name, path))
			.OnError(x => _log.LogError(x, "Could not create workspace '{Name}' in '{Path}'", name, path));

		if (!result.IsError)
			return await LoadAsync(name);

		return result;
	}

	public Task<Result<ImmutableArray<Abstractions.Workspace>>> ListAsync()
	{
		return Result.TryAsync(async () =>
		{
			var workspaces = new List<Abstractions.Workspace>();

			var recents = await ReadRecent();
			foreach (var recent in recents)
			{
				(await LoadAsync(recent))
					.On(value => workspaces.Add(value))
					.OnError(ex => _log.LogError(ex, "Could not load recent workspace '{Path}'", recent));
			}

			return workspaces.ToImmutableArray();
		});
	}

	public Task<Result<Abstractions.Workspace>> CreateOrLoadAsync(string name, string path)
	{
		if (Exists(path))
			return LoadAsync(path);

		return CreateAsync(name, path);
	}

	public Task<Result<Abstractions.Workspace>> CreateOrLoadAsync(string path)
	{
		if (Exists(path))
			return LoadAsync(path);

		return CreateAsync(Path.GetFileNameWithoutExtension(path), path);
	}

	public async Task<Result<Abstractions.Workspace>> SaveAsync(Abstractions.Workspace workspace)
	{
		return (await Result.TryAsync(async () =>
			{
				workspace = workspace with { LastModified = DateTime.Now };
				await File.WriteAllTextAsync(workspace.Path, EncodeWorkspace(workspace));
				return workspace;
			}))
			.On(x => _log.LogInformation("Saved workspace '{Name}' to '{Path}'", workspace.Name, workspace.Path))
			.OnError(x =>
				_log.LogError(x, "Could not save workspace '{Name}' to '{Path}'", workspace.Name, workspace.Path));
	}

	public void Delete(Abstractions.Workspace workspace)
	{
		if (!Exists(workspace.Path))
			return;

		File.Delete(workspace.Path);
		_log.LogInformation("Deleted workspace '{Name}' in '{Path}'", workspace.Name, workspace.Path);
	}
	
	public async Task<Result<Abstractions.Web>> CreateWebAsync(Abstractions.Workspace workspace, string name)
	{
		name = name.Trim();
		
		if(string.IsNullOrWhiteSpace(name))
			return new Exception("Name cannot be empty.");
		
		if (workspace.Webs.Any(x => string.Equals(x.Name.Trim(), name, StringComparison.InvariantCultureIgnoreCase)))
			return new Exception($"A web with the name '{name}' already exists.");
		
		workspace = workspace with { Webs = workspace.Webs.Add(new Abstractions.Web(name)) };

		return (await SaveAsync(workspace))
			.On(_ => _log.LogInformation("Created web '{Web}' in '{Workspace}'", name, workspace.Name))
			.OnError(ex => _log.LogError(ex, "Could not create web '{Web}' in '{Workspace}'", name, workspace.Name))
			.Pipe(x => x.Webs.First(y => y.Name == name));
	}

	public async Task<Result<Abstractions.Workspace>> LoadAsync(string path)
	{
		if (!Exists(path))
			return new FileNotFoundException("Workspace file could not be found", path);

		return (await Result.TryAsync(async () =>
			{
				var json = await File.ReadAllTextAsync(path);
				var workspace = DecodeWorkspace(json);

				if (workspace.Webs.Length == 0) workspace = workspace with { Webs = ImmutableArray.Create(DefaultWeb) };

				workspace = workspace with { Path = path };

				foreach (var dependency in workspace.Addons)
				{
					var extension = _addons.Addons.First(x =>
						x.Name == dependency.Name && x.Version >= dependency.MinVersion);
					// var structures = _nodes.ExtractStructures(extension.GetType().Assembly);

					//workspace = workspace with { Structures = workspace.Structures.AddRange(structures) };
				}

				//workspace = workspace with { Types = _nodes.GenerateTypeDefinitions(workspace.Structures) };

				// Clean up double connections in the webs
				// workspace = workspace with
				// {
				//     Webs = workspace.Webs.Select(web =>
				// {
				//     var connections = web.Connections.DistinctBy(x => (x.Source.Method, x.Target.Method)).ToImmutableArray();
				//     return web with { Connections = connections };
				// }).ToImmutableArray()
				// };

				await MarkRecent(workspace);

				return workspace;
			}))
			.On(x => _log.LogInformation("Loaded workspace '{Name}' with {Webs} webs from '{Path}'", x.Name,
				x.Webs.Length,
				path))
			.OnError(x => _log.LogError(x, "Could not load workspace file from '{Path}'", path));
	}

	public bool Exists(string path)
	{
		return File.Exists(path);
	}

	private static async Task MarkRecent(Abstractions.Workspace workspace)
	{
		var recents = (await ReadRecent())
			.Add(workspace.Path)
			.Distinct()
			.Take(10)
			.ToImmutableArray();

		await WriteRecent(recents);
	}

	private static async Task WriteRecent(ImmutableArray<string> workspaces)
	{
		await using var fileStream = new FileStream(RecentPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
		
		await using var writer = new StreamWriter(fileStream);
		foreach (var workspace in workspaces)
		{
			await writer.WriteLineAsync(workspace);
		}

		await writer.FlushAsync();
	}

	private static async Task<ImmutableArray<string>> ReadRecent()
	{
		await using var fileStream = new FileStream(RecentPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);

		using var reader = new StreamReader(fileStream);
		var workspaces = new List<string>();
		while (await reader.ReadLineAsync() is { } line)
		{
			workspaces.Add(line);
		}
		 
		reader.Close();
		return workspaces.ToImmutableArray();
	}
	
	
	private static string EncodeWorkspace(Abstractions.Workspace workspace)
	{
		//workspace = workspace with { Path = string.Empty, Structures = ImmutableArray<NodeStructure>.Empty, Types = ImmutableArray<NodeTypeDefinition>.Empty };
		return JsonConvert.SerializeObject(workspace, Formatting.Indented);
	}

	private static Abstractions.Workspace DecodeWorkspace(string encoded)
	{
		return JsonConvert.DeserializeObject<Abstractions.Workspace>(encoded);
	}
}
