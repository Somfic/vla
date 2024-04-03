using System.Collections.Immutable;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Somfic.Common;
using Vla.Abstractions;
using Vla.Addons.Math;
using Vla.Workspace;

namespace Vla.Services;

public class WorkspaceService
{
	private readonly string _recentPath =
		Path.GetFullPath(Path.Combine("Vla-appdata", "recent workspaces.txt"));
	
	// private static readonly Web DefaultWeb = new("Pythagorean theorem")
	// {
	// 	Instances =
	// 	[
	// 		new NodeInstance
	// 		{
	// 			Guid = Guid.Parse("00000000-0000-0000-0000-000000000001"),
	// 			Type = typeof(MathNode),
	// 			Properties = [new NamedValue("Mode", "Mode", MathNode.MathMode.Power)],
	// 			Inputs = [new NamedValue("power.value", "Value", 3), new NamedValue("power.base", "Base", 2)]
	// 		},
	// 		new NodeInstance
	// 		{
	// 			Guid = Guid.Parse("00000000-0000-0000-0000-000000000002"),
	// 			Type = typeof(MathNode),
	// 			Properties = [new NamedValue("Mode", "Mode", MathNode.MathMode.Power)],
	// 			Inputs = [new NamedValue("power.value", "Value", 4), new NamedValue("power.base", "Base", 2)]
	// 		},
	// 		new NodeInstance
	// 		{
	// 			Guid = Guid.Parse("00000000-0000-0000-0000-000000000003"),
	// 			Type = typeof(MathNode),
	// 			Properties = [new NamedValue("Mode", "Mode", MathNode.MathMode.Add)]
	// 		},
	// 		new NodeInstance
	// 		{
	// 			Guid = Guid.Parse("00000000-0000-0000-0000-000000000004"),
	// 			Type = typeof(MathNode),
	// 			Properties = [new NamedValue("Mode", "Mode", MathNode.MathMode.SquareRoot)]
	// 		}
	// 	],
	// 	Connections =
	// 	[
	// 		new NodeConnection("00000000-0000-0000-0000-000000000001", "power.result",
	// 			"00000000-0000-0000-0000-000000000003", "add.a"),
	// 		new NodeConnection("00000000-0000-0000-0000-000000000002", "power.result",
	// 			"00000000-0000-0000-0000-000000000003", "add.b"),
	// 		new NodeConnection("00000000-0000-0000-0000-000000000003", "add.result",
	// 		"00000000-0000-0000-0000-000000000004", "squareRoot.value")
	// 	]
	// };

	private static ImmutableArray<NodeInstance> DefaultWebInstances =
	[
		new NodeInstance()
		{
			Guid = Guid.Parse("00000000-0000-0000-0000-000000000000"),
			Type = typeof(MathNode),
			Inputs = [new("add.b", "B", 1)],
			Properties = [new NamedValue("Mode", "Mode", MathNode.MathMode.Add)]
		}
	];
	
	private static ImmutableArray<NodeConnection> DefaultWebConnections = [
		new NodeConnection("00000000-0000-0000-0000-000000000000", "add.result", "00000000-0000-0000-0000-000000000000", "add.a")
	];

	private readonly AddonService _addons;

	private readonly TypeService _types;

	private readonly ILogger<WorkspaceService> _log;

	public WorkspaceService(ILogger<WorkspaceService> log, AddonService addons, TypeService types)
	{
		_log = log;
		_addons = addons;
		_types = types;

		var appPath = Path.GetDirectoryName(_recentPath);

		if (!string.IsNullOrEmpty(appPath))
			Directory.CreateDirectory(appPath);
	}
	
	public Result<Abstractions.Workspace> Create(string path) =>
		Create(Path.GetFileNameWithoutExtension(path), path);
	
	public Result<Abstractions.Workspace> Create(string name, string path)
	{
		path = Path.GetFullPath(path);
		
		if (Exists(path))
			return new Exception($"A workspace already exists at {path}");

		var result = Result.Try(() =>
			{
				var workspace = new Abstractions.Workspace(name)
				{
					Name = name,
					Path = path,
					Created = DateTime.Now,
					LastModified = DateTime.Now,
					Addons = [("Core", new Version(0, 0, 0))]
				};
				LazyFile.Write(path, EncodeWorkspace(workspace));
				return workspace;
			})
			.On(x => _log.LogInformation("Created workspace '{Name}' in '{Path}'", x.Name, path))
			.OnError(x => _log.LogError(x, "Could not create workspace '{Name}' in '{Path}'", name, path));

		if (!result.IsError)
			return Load(path);

		return result;
	}

	public Result<ImmutableArray<Abstractions.Workspace>> List()
	{
		return Result.Try( () =>
		{
			var workspaces = new List<Abstractions.Workspace>();

			var recents = LazyFile.Read(_recentPath).Split(Environment.NewLine).Where(Exists);
			foreach (var recent in recents)
			{
				Load(recent)
					.On(value => workspaces.Add(value))
					.OnError(_ => UnmarkRecent(recent))
					.OnError(ex => _log.LogError(ex, "Could not load recent workspace '{Path}'", recent));
			}

			return workspaces.ToImmutableArray();
		});
	}

	public Result<Abstractions.Workspace> CreateOrLoad(string name, string path)
	{
		if (Exists(path))
			return Load(path);

		return Create(name, path);
	}

	public Result<Abstractions.Workspace> CreateOrLoad(string path)
	{
		return CreateOrLoad(Path.GetFileNameWithoutExtension(path), path);
	}

	public Result<Abstractions.Workspace> Save(Abstractions.Workspace workspace)
	{
		return Result.Try(() =>
			{
				workspace = workspace with { LastModified = DateTime.Now };
				LazyFile.Write(workspace.Path, EncodeWorkspace(workspace));
				return workspace;
			})
			.On(x => _log.LogInformation("Saved workspace '{Name}' to '{Path}'", workspace.Name, workspace.Path))
			.OnError(x =>
				_log.LogError(x, "Could not save workspace '{Name}' to '{Path}'", workspace.Name, workspace.Path));
	}

	public void Delete(Abstractions.Workspace workspace)
	{
		if (!Exists(workspace.Path))
			return;

		LazyFile.Delete(workspace.Path);

		UnmarkRecent(workspace.Path);
		
		_log.LogInformation("Deleted workspace '{Name}' in '{Path}'", workspace.Name, workspace.Path);
	}
	
	public Result<Web> CreateWeb(Abstractions.Workspace workspace, string name)
	{
		name = name.Trim();
		
		if(string.IsNullOrWhiteSpace(name))
			return new Exception("Name cannot be empty.");
		
		if (workspace.Webs.Any(x => string.Equals(x.Name.Trim(), name, StringComparison.InvariantCultureIgnoreCase)))
			return new Exception($"A web with the name '{name}' already exists.");
		
		workspace = workspace with { Webs = workspace.Webs.Add(new Web(workspace.Path, name)) };

		return Save(workspace)
			.On(_ => _log.LogInformation("Created web '{Web}' in '{Workspace}'", name, workspace.Name))
			.OnError(ex => _log.LogError(ex, "Could not create web '{Web}' in '{Workspace}'", name, workspace.Name))
			.Pipe(x => x.Webs.First(y => y.Name == name));
	}
	
	public Result<Web> UpdateWeb(Web web)
	{
		var workspaceResult = List()
			.Pipe(x => x.FirstOrDefault(y => y.Path == web.WorkspacePath));

		if (workspaceResult.IsError)
			return workspaceResult.Error.Expect();
		
		var workspace = workspaceResult.Expect();
		
		if (!workspace.Webs.Any(x => x.Name == web.Name))
			return new Exception($"Web '{web.Name}' does not exist in '{workspace.Name}'");

		workspace = workspace with { Webs = workspace.Webs.Where(x => x.Name != web.Name).Append(web).ToImmutableArray() };

		return Save(workspace)
			.On(_ => _log.LogInformation("Updated web '{Web}' in '{Workspace}'", web.Name, workspace.Name))
			.OnError(ex => _log.LogError(ex, "Could not update web '{Web}' in '{Workspace}'", web.Name, workspace.Name))
			.Pipe(x => x.Webs.First(y => y.Name == web.Name));
	}

	public Result<Abstractions.Workspace> Load(string path)
	{
		if (!Exists(path))
			return new FileNotFoundException("Workspace file could not be found", path);

		return Result.Try( () =>
			{
				var json = LazyFile.Read(path);
				var workspace = DecodeWorkspace(json);

				if (workspace.Webs.Length == 0) workspace = workspace with { Webs = ImmutableArray.Create(new Web(workspace.Path, "Default web") { Instances = DefaultWebInstances, Connections = DefaultWebConnections}) };

				workspace = workspace with { Path = path };

				foreach (var dependency in workspace.Addons)
				{
					 var extension = _addons.Addons.First(x =>
					 	x.Name == dependency.Name && x.Version == dependency.Version);
					 var types = _types.GenerateDefinition()

					workspace = workspace with { Structures = workspace.Structures.AddRange(structures) };
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

				MarkRecent(workspace);

				return workspace;
			})
			.On(x => _log.LogInformation("Loaded workspace '{Name}' with {Webs} webs from '{Path}'", x.Name,
				x.Webs.Length,
				path))
			.OnError(x => _log.LogError(x, "Could not load workspace file from '{Path}'", path));
	}

	public bool Exists(string path)
	{
		return LazyFile.Exists(path);
	}
	
	private void UnmarkRecent(string path)
	{
		var newRecents =
			string.Join(Environment.NewLine,
				LazyFile
					.Read(_recentPath)
					.Split(Environment.NewLine)
					.Distinct()
					.Where(x => x != path)
					.Where(Exists));
		
		LazyFile.Write(_recentPath, newRecents);
		
		_log.LogDebug("Unmarked workspace '{Name}' as recent", path);

	}

	private void MarkRecent(Abstractions.Workspace workspace)
	{
		var newRecents =
			string.Join(Environment.NewLine,
				LazyFile
					.Read(_recentPath)
					.Split(Environment.NewLine)
					.Append(workspace.Path)
					.Distinct()
					.Where(Exists));
		
		LazyFile.Write(_recentPath, newRecents);

		_log.LogDebug("Marked workspace '{Name}' as recent", workspace.Name);
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
