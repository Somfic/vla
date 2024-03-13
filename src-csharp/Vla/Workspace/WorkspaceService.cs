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
	private static readonly string RecentPath =
		Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Vla", "recent workspaces");

	private static readonly Web DefaultWeb = new("Pythagorean theorem")
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
	private readonly NodeService _nodes;

	public WorkspaceService(ILogger<WorkspaceService> log, NodeService nodes, AddonService addons)
	{
		_log = log;
		_nodes = nodes;
		_addons = addons;
	}

	public Task<Result<ImmutableArray<Abstractions.Workspace>>> ListRecentAsync()
	{
		return Result.TryAsync(async () =>
		{
			var file = new FileStream(RecentPath, FileMode.OpenOrCreate);

			var workspaces = new List<Abstractions.Workspace>();

			using var reader = new StreamReader(file);
			while (await reader.ReadLineAsync() is { } path)
				if (Exists(path))
					(await LoadAsync(path)).On(workspaces.Add);

			await file.FlushAsync();
			file.Close();

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

	public async Task SaveAsync(Abstractions.Workspace workspace)
	{
		(await Result.TryAsync(async () =>
			{
				workspace = workspace with { LastModified = DateTime.Now };
				await File.WriteAllTextAsync(workspace.Path, EncodeWorkspace(workspace));
				return true;
			})).On(x => _log.LogInformation("Saved workspace {Name} at {Path}", workspace.Name, workspace.Path))
			.OnError(x =>
				_log.LogWarning(x, "Could not save workspace {Name} at {Path}", workspace.Name, workspace.Path));
	}

	public void Delete(Abstractions.Workspace workspace)
	{
		if (!Exists(workspace.Path))
			return;

		File.Delete(workspace.Path);
		_log.LogInformation("Deleted workspace {Name} at {Path}", workspace.Name, workspace.Path);
	}

	private async Task<Result<Abstractions.Workspace>> CreateAsync(string name, string path)
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
			.On(x => _log.LogInformation("Created workspace {Name} at {Path}", x.Name, path))
			.OnError(x => _log.LogWarning(x, "Could not create workspace {Name} at {Path}", name, path));

		if (!result.IsError)
			return await LoadAsync(name);

		return result;
	}

	private async Task<Result<Abstractions.Workspace>> LoadAsync(string path)
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
				//     var connections = web.Connections.DistinctBy(x => (x.Source.Id, x.Target.Id)).ToImmutableArray();
				//     return web with { Connections = connections };
				// }).ToImmutableArray()
				// };

				await MarkRecent(workspace);

				return workspace;
			}))
			.On(x => _log.LogInformation("Loaded workspace {Name} with {Webs} webs at {Path}", x.Name, x.Webs.Length,
				path))
			.OnError(x => _log.LogWarning(x, "Could not load workspace file at {Path}", path));
	}

	public bool Exists(string path)
	{
		return File.Exists(path);
	}

	private static async Task MarkRecent(Abstractions.Workspace workspace)
	{
		var recents = await File.ReadAllLinesAsync(RecentPath);
		if (recents.Contains(workspace.Path))
			return;

		await File.AppendAllTextAsync(RecentPath, $"{workspace.Path}\n");

		// TODO: Add this to configuration
		if (recents.Length > 10)
		{
			var toRemove = recents.Length - 10;
			await File.WriteAllLinesAsync(RecentPath, recents.Skip(toRemove));
		}
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