using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Somfic.Common;
using Vla.Abstractions;
using Vla.Input;
using Vla.Nodes;
using Vla.Server;
using Vla.Server.Messages;
using Vla.Workspace;

var host = Host.CreateDefaultBuilder()
    .ConfigureServices(s =>
    {
        s.AddHttpClient();
        s.AddSingleton<WebsocketService>();
        s.AddSingleton<NodeService>();
        s.AddSingleton<InputService>();
        s.AddSingleton<VariableManager>();
        s.AddSingleton<WorkspaceService>();
        s.AddSingleton<AddonService>();
        s.UseAddons(AddonService.Path);
    }).ConfigureLogging(s =>
    {
        s.SetMinimumLevel(LogLevel.Debug);
    })
    .Build();

var addons = host.Services.GetRequiredService<AddonService>();
addons.RegisterAddons();

var nodes = host.Services.GetRequiredService<NodeService>();
var workspaces = host.Services.GetRequiredService<WorkspaceService>();
var server = host.Services.GetRequiredService<WebsocketService>();

if (!workspaces.Exists("Workspace"))
    await workspaces.CreateOrLoadAsync("Workspace");

// Start the websocket server
await server.StartAsync();

server.ClientConnected.OnChange(async c =>
{
    await server.SendAsync(c, new WorkspacesMessage(await workspaces.ListAsync()));
});

server.MessageReceived.OnChange(async args =>
{
    var (client, json) = args;

    var message = JObject.Parse(json);

    switch (message["id"]?.Value<string>()?.ToLower())
    {
        case "update-web":
            var workspaceChanged = JsonConvert.DeserializeObject<UpdateWebMessage>(json);
            var workspace = (await workspaces.CreateOrLoadAsync(workspaceChanged.WorkspacePath))
                .On(x =>
                {
                    
                });
                
            break;
    }
});

await server.MarkReady();
while (server.IsRunning)
    await Task.Delay(1000);