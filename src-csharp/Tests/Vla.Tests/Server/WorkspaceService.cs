using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Vla.Nodes;
using Vla.Server.Messages.Requests;
using Vla.Websocket;

namespace Vla.Tests.Server;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

public class WorkspaceService
{
	private const string WorkspacePath = "workspaces";

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
		
		var result = await workspaces.CreateAsync(id, path);
		
		Console.WriteLine(result);
		
		Assert.That(result.IsValue, Is.True);
		
		result = await workspaces.CreateAsync(id, path);
		
		Assert.That(result.IsError, Is.True);
	}

	private (string id, string path) RandomIdAndPath()
	{
		var id = Guid.NewGuid().ToString();
		var path = Path.Combine(WorkspacePath, id);
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