using System.Collections.Immutable;
using System.Threading.Channels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Somfic.Common;
using Vla.Abstractions;
using Vla.Addon.Services;
using Vla.Addons;
using Vla.Engine;
using Vla.Server;
using Vla.Services;
using Vla.Websocket;
using Vla.Workspace;

Directory.CreateDirectory(AddonService.Path);

var host = Host.CreateDefaultBuilder()
	.ConfigureServices(s =>
	{
		s.AddHttpClient();
		s.AddSingleton<IWebsocketService, WebsocketService>();
		//s.AddSingleton<NodeService>();
		s.AddSingleton<InputService>();
		s.AddSingleton<IVariableManager, VariableManager>();
		s.AddSingleton<WorkspaceService>();
		s.AddSingleton<AddonService>();
		s.AddSingleton<NodeEngine>();
		s.AddSingleton<ServerService>();
		s.UseAddon<CoreAddon>();
		s.UseAddons(AddonService.Path);
	}).ConfigureLogging(s => { s.SetMinimumLevel(LogLevel.Information); })
	.Build();

var addons = host.Services.GetRequiredService<AddonService>();
addons.RegisterAddons();

var server = host.Services.GetRequiredService<ServerService>();
var engine = host.Services.GetRequiredService<NodeEngine>();
var workspaces = host.Services.GetRequiredService<WorkspaceService>();

server.AddMethods<Vla.Server.Methods.WorkspaceMethods>();
server.AddMethods<Vla.Server.Methods.WebMethods>();

await server.StartAsync();

while (true)
{
	var workspacesResult = workspaces.List();

	if (workspacesResult.IsValue)
	{
		foreach (var workspace in workspacesResult.Value.Expect())
		{
			foreach (var web in workspace.Webs)
			{
				engine.LoadWeb(web);

				await engine.Tick();

				var updatedWeb = engine.SaveWeb();

				workspaces.UpdateWeb(updatedWeb);
			}
		}

		await server.SendToAll("workspace list");
	}
	
	await server.Tick();
	
	await Task.Delay(250);
}

