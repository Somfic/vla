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
	public async Task WorkspaceService_Create_ThrowsErrorIfAlreadyExists()
	{
		var workspaces = CreateWorkspaceService();

		var (id, path) = RandomIdAndPath();

		var workspace = (await workspaces.CreateAsync(id, path)).Expect();
		
		var workspaceResult = await workspaces.CreateAsync(id, path);
		
		Assert.That(workspaceResult.IsError, Is.True);
	}
	
	[Test]
	public async Task WorkspaceService_List_ReturnsWorkspaces()
	{
		var workspaces = CreateWorkspaceService();

		var (id, path) = RandomIdAndPath();
		
		var workspace = (await workspaces.CreateAsync(id, path)).Expect();

		var recents = (await workspaces.ListAsync()).Expect();
		
		Assert.That(recents.Any(x => x.Name == id), Is.True);
	}
	
	[Test]
	public async Task WorkSpaceService_Load_LoadsWorkspace()
	{
		var workspaces = CreateWorkspaceService();

		var (id, path) = RandomIdAndPath();

		(await workspaces.CreateAsync(id, path)).Expect();
		var workspace = (await workspaces.LoadAsync(path)).Expect();
		
		Assert.That(workspace.Name, Is.EqualTo(id));
	}
	
	[Test]
	public async Task WorkspaceService_CreateWeb_AddsWeb()
	{
		var workspaces = CreateWorkspaceService();

		var (id, path) = RandomIdAndPath();

		var workspace = (await workspaces.CreateAsync(id, path)).Expect();
		var web = (await workspaces.CreateWebAsync(workspace, "test web")).Expect();
		
		Assert.That(web.Name, Is.EqualTo("test web"));

		workspace = (await workspaces.LoadAsync(path)).Expect();
		Assert.That(workspace.Webs.Any(x => x.Name == "test web"), Is.True);
	}
	
	[Test]
	public async Task WorkspaceService_CreateOrLoad_CreatesOnNonExistent()
	{
		var workspaces = CreateWorkspaceService();

		var (id, path) = RandomIdAndPath();

		var oldList = (await workspaces.ListAsync()).Expect();

		var workspace = (await workspaces.CreateOrLoadAsync(id, path)).Expect();
		
		var newList = (await workspaces.ListAsync()).Expect();

		Assert.That(oldList.Any(x => x.Name == workspace.Name), Is.False);
		Assert.That(newList.Any(x => x.Name == workspace.Name), Is.True);
	}
	
	[Test]
	public async Task WorkspaceService_CreateOrLoad_LoadsOnExistent()
	{
		var workspaces = CreateWorkspaceService();

		var (id, path) = RandomIdAndPath();

		var workspace1 = (await workspaces.CreateAsync(path)).Expect();
		
		var oldList = (await workspaces.ListAsync()).Expect();

		var workspace2 = (await workspaces.CreateOrLoadAsync(id, path)).Expect();
		
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
				l.SetMinimumLevel(LogLevel.Trace);
			})
			.Build()
			.Services;

		return services.GetRequiredService<Workspace.WorkspaceService>();
	}
}