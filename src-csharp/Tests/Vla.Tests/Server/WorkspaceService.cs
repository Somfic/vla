using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Somfic.Common;
using Vla.Nodes;
using Vla.Server.Messages.Requests;
using Vla.Websocket;

namespace Vla.Tests.Server;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

public class WorkspaceService
{
	private static readonly string WorkspacePath = Path.GetFullPath(Path.Combine("workspaces"));

	// Before runnning the tests, make sure to delete the workspaces folder (NUnit)
	[OneTimeSetUp]
	public void Setup()
	{
		if (Directory.Exists(WorkspacePath))
			Directory.Delete(WorkspacePath, true);

		Directory.CreateDirectory(WorkspacePath);
	}
	
	
    [Test]
	public void WorkspaceService_Create_ThrowsErrorIfAlreadyExists()
	{
		var workspaces = CreateWorkspaceService();

		var (id, path) = RandomIdAndPath();

		var workspace = workspaces.Create(id, path).Expect();
		
		var workspaceResult =  workspaces.Create(id, path);
		
		Assert.That(workspaceResult.IsError, Is.True);
	}
	
	[Test]
	public void WorkspaceService_List_ReturnsWorkspaces()
	{
		var workspaces = CreateWorkspaceService();

		var (id, path) = RandomIdAndPath();
		
		var workspace = workspaces.Create(id, path).Expect();

		var recents = workspaces.List().Expect();
		
		Assert.That(recents.Any(x => x.Name == id), Is.True);
	}
	
	[Test]
	public void WorkSpaceService_Load_LoadsWorkspace()
	{
		var workspaces = CreateWorkspaceService();

		var (id, path) = RandomIdAndPath();

		workspaces.Create(id, path).Expect();
		var workspace = workspaces.Load(path).Expect();
		
		Assert.That(workspace.Name, Is.EqualTo(id));
	}
	
	[Test]
	public void WorkspaceService_CreateWeb_AddsWeb()
	{
		var workspaces = CreateWorkspaceService();

		var (id, path) = RandomIdAndPath();

		var workspace = workspaces.Create(id, path).Expect();
		var web = workspaces.CreateWeb(workspace, "test web").Expect();
		
		Assert.That(web.Name, Is.EqualTo("test web"));

		workspace = workspaces.Load(path).Expect();
		Assert.That(workspace.Webs.Any(x => x.Name == "test web"), Is.True);
	}
	
	[Test]
	public void WorkspaceService_CreateOrLoad_CreatesOnNonExistent()
	{
		var workspaces = CreateWorkspaceService();

		var (id, path) = RandomIdAndPath();

		var oldList = workspaces.List().Expect();

		var workspace = workspaces.CreateOrLoad(id, path).Expect();
		
		var newList = workspaces.List().Expect();

		Assert.That(oldList.Any(x => x.Name == workspace.Name), Is.False);
		Assert.That(newList.Any(x => x.Name == workspace.Name), Is.True);
	}
	
	[Test]
	public void WorkspaceService_CreateOrLoad_LoadsOnExistent()
	{
		var workspaces = CreateWorkspaceService();

		var (id, path) = RandomIdAndPath();

		var workspace1 = workspaces.Create(path).Expect();
		
		var oldList = workspaces.List().Expect();

		var workspace2 = workspaces.CreateOrLoad(id, path).Expect();
		
		Assert.That(workspace1.Name, Is.EqualTo(workspace2.Name));
	}
	
	
	private (string id, string path) RandomIdAndPath()
	{
		var id = Guid.NewGuid().ToString();
		var path = Path.Combine(WorkspacePath, id + ".vla");
		return (id, path);
	}
    
	private Workspace.WorkspaceService CreateWorkspaceService()
	{
		var services = Host.CreateDefaultBuilder()
			.ConfigureServices(s =>
			{
				s.AddSingleton<Workspace.WorkspaceService>();
				s.AddSingleton<AddonService>();
			})
			.ConfigureLogging(l =>
			{
				l.AddConsole();
				l.SetMinimumLevel(LogLevel.Warning);
			})
			.Build()
			.Services;

		return services.GetRequiredService<Workspace.WorkspaceService>();
	}
}