using System.Collections.Immutable;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Vla.Abstractions;
using Vla.Addon.Services;
using Vla.Addons;
using Vla.Engine;
using Vla.Input;
using Vla.Nodes;
using Vla.Server;
using Vla.Server.Messages;
using Vla.Workspace;

Directory.CreateDirectory(AddonService.Path);

var host = Host.CreateDefaultBuilder()
    .ConfigureServices(s =>
    {
        s.AddHttpClient();
        s.AddSingleton<WebsocketService>();
        s.AddSingleton<NodeService>();
        s.AddSingleton<InputService>();
        s.AddSingleton<IVariableManager, VariableManager>();
        s.AddSingleton<WorkspaceService>();
        s.AddSingleton<AddonService>();
        s.AddSingleton<NodeEngine>();
        s.UseAddon<CoreAddon>();
        s.UseAddons(AddonService.Path);
    }).ConfigureLogging(s =>
    {
        s.SetMinimumLevel(LogLevel.Debug);
    })
    .Build();

var addons = host.Services.GetRequiredService<AddonService>();
addons.RegisterAddons();

var engine = host.Services.GetRequiredService<NodeEngine>();
var nodes = host.Services.GetRequiredService<NodeService>();
var workspaces = host.Services.GetRequiredService<WorkspaceService>();
var server = host.Services.GetRequiredService<WebsocketService>();

// Start the websocket server
await server.StartAsync();

server.ClientConnected.OnChange(async c =>
{
    await workspaces.CreateOrLoadAsync("Workspace");
    await server.SendAsync(c, new WorkspacesMessage(await workspaces.ListAsync()));
});

server.MessageReceived.OnChange(async args =>
{
    var (client, json) = args;

    var message = JObject.Parse(json);

    switch (message["id"]?.Value<string>()?.ToLower())
    {
        case "save-workspace":
        {
            var runWorkspace = JsonConvert.DeserializeObject<RunWorkspaceMessage>(json);
            var workspace = runWorkspace.Workspace;
            await workspaces.SaveAsync(workspace);
            
            engine.SetStructures(workspace.Structures);
            engine.SetGraph(workspace.Webs.SelectMany(x => x.Instances).ToImmutableArray(), workspace.Webs.SelectMany(x => x.Connections).ToImmutableArray());
            break;
        }
    }
});

await server.MarkReady();

while (server.IsRunning)
{
    var results = engine.Tick();
    await server.BroadcastAsync(new ExecutionResultMessage(results));
    await Task.Delay(1000);
}