using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Vla.Abstractions;
using Vla.Addon.Services;
using Vla.Addons;
using Vla.Engine;
using Vla.Input;
using Vla.Nodes;
using Vla.Server;
using Vla.Websocket;
using Vla.Workspace;

Directory.CreateDirectory(AddonService.Path);

var host = Host.CreateDefaultBuilder()
	.ConfigureServices(s =>
	{
		s.AddHttpClient();
		s.AddSingleton<IWebsocketService, WebsocketService>();
		s.AddSingleton<NodeService>();
		s.AddSingleton<InputService>();
		s.AddSingleton<IVariableManager, VariableManager>();
		s.AddSingleton<WorkspaceService>();
		s.AddSingleton<AddonService>();
		s.AddSingleton<NodeEngine>();
		s.UseAddon<CoreAddon>();
		s.UseAddons(AddonService.Path);
	}).ConfigureLogging(s => { s.SetMinimumLevel(LogLevel.Debug); })
	.Build();

var addons = host.Services.GetRequiredService<AddonService>();
addons.RegisterAddons();

var server = host.Services.GetRequiredService<ServerService>();
var engine = host.Services.GetRequiredService<NodeEngine>();

server.AddMethods<Vla.Server.Methods.Workspace>();

server.OnTick(async () =>
{
	await engine.Tick();
});

await server.StartAsync();