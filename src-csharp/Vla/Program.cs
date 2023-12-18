using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Somfic.Common;
using Vla.Abstractions;
using Vla.Abstractions.Web;
using Vla.Input;
using Vla.Nodes;
using Vla.Nodes.Math;
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
    })
    .Build();

var nodes = host.Services.GetRequiredService<NodeService>();
var workspaces = host.Services.GetRequiredService<WorkspaceService>();
var server = host.Services.GetRequiredService<WebsocketService>();

if (!workspaces.Exists("Workspace"))
    await workspaces.CreateOrLoadAsync("Workspace");

nodes.RegisterStructures(typeof(BasicMathNode).Assembly);

// Start the websocket server
await server.StartAsync();

server.ClientConnected.OnChange(async c =>
{
    await server.SendAsync(c, new NodesStructureMessage(nodes.Structures, nodes.GenerateTypeDefinitions()));
    await server.SendAsync(c, new WorkspacesMessage(await workspaces.ListAsync()));
});

server.MessageReceived.OnChange(async args =>
{
    var (client, json) = args;

    var message = JObject.Parse(json);

    switch (message["id"]?.Value<string>()?.ToLower())
    {
        case "runweb":
            var runWeb = JsonConvert.DeserializeObject<RunWebMessage>(json);
            WebExtensions.Validate(runWeb
                    .Web, nodes.Structures)
                .Pipe(x => nodes.Execute(x))
                .On(async x => await server.SendAsync(client, new WebResultMessage(x)))
                .OnError(Console.WriteLine);
            break;

        case "workspacechanged":
            var workspaceChanged = JsonConvert.DeserializeObject<WorkspaceChangedMessage>(json);
            await workspaces.SaveAsync(workspaceChanged.Workspace);
            break;
    }
});

await server.MarkReady();
Thread.Sleep(-1);