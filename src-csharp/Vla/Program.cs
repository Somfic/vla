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
using Vla.Web;
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
        s.AddSingleton<ExtensionsService>();
        s.UseExtensions(ExtensionsService.Path);
    }).ConfigureLogging(s =>
    {
        s.SetMinimumLevel(LogLevel.Debug);
    })
    .Build();

var extensions = host.Services.GetRequiredService<ExtensionsService>();
extensions.RegisterExtensions();

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
        case "workspacechanged":
            var workspaceChanged = JsonConvert.DeserializeObject<WorkspaceChangedMessage>(json);
            await workspaces.SaveAsync(workspaceChanged.Workspace);
            break;
    }
});

await extensions.OnStart();

await server.MarkReady();
while (server.IsRunning)
    await Task.Delay(1000);
await extensions.OnStop();