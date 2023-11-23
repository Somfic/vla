using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Somfic.Common;
using Vla.Input;
using Vla.Nodes;
using Vla.Nodes.Connection;
using Vla.Nodes.Instance;
using Vla.Nodes.Web;
using Vla.Server;
using Vla.Server.Messages;
using Vla.Voice;

var host = Host.CreateDefaultBuilder()
    .ConfigureServices(s =>
    {
        s.AddHttpClient();
        s.AddSingleton<RecogniserService>();
        s.AddSingleton<SpeechProcessorService>();
        s.AddSingleton<WebsocketService>();
        s.AddSingleton<NodeService>();
        s.AddSingleton<InputService>();
    })
    .Build();

var server = host.Services.GetRequiredService<WebsocketService>();
await server.StartAsync();

var recogniser = host.Services.GetRequiredService<RecogniserService>();
var processor = host.Services.GetRequiredService<SpeechProcessorService>();
var node = host.Services.GetRequiredService<NodeService>();

await recogniser.InitialiseAsync();
await processor.InitialiseAsync();

recogniser.Progress.OnChange(e =>
{
    Console.WriteLine($"{e.Percentage}%: {e.Label}");
    server.BroadcastAsync(new Progress(e.Percentage, e.Label)).GetAwaiter().GetResult();
});

recogniser.Recognised.OnChange(e =>
{
    var processedText = processor.Process(e.Text);
    server.BroadcastAsync(new RecogniserRecognised(processedText)).GetAwaiter().GetResult();
});

recogniser.PartlyRecognised.OnChange(e =>
{
    server.BroadcastAsync(new RecogniserRecognisedPartial(e)).GetAwaiter().GetResult();
});

node.Register<MathNode>()
    .Register<NumberConstantNode>()
    .Register<PrinterNode>()
    .Register<MathModulo>()
    .Register<ConditionalNode>();

var constantInstance1 = new NodeInstance()
    .From<NumberConstantNode>()
    .WithProperty("Value", 2m)
    .WithPosition(0, 0);

var constantInstance2 = new NodeInstance()
    .From<NumberConstantNode>()
    .WithProperty("Value", 2m);

var moduloInstance = new NodeInstance()
    .From<MathModulo>()
    .WithPosition(10, 0);

var conditionalInstance = new NodeInstance()
    .From<ConditionalNode>();

var printInstance = new NodeInstance()
    .From<PrinterNode>();

var constant1ToMathA = new NodeConnection()
    .From(constantInstance1, "value")
    .To(moduloInstance, "modulo");

var instances = new[] { constantInstance1, moduloInstance };
var connections = new[] { constant1ToMathA };
var web = new Web()
    .WithInstances(instances)
    .WithConnections(connections)
    .Validate(node.Structures)
    .OnError(Console.WriteLine);

server.ClientConnected.OnChange(async c =>
{
    await server.SendAsync(c, new NodesStructureMessage(node.Structures, node.GenerateTypeDefinitions()));
    web.On(async x => await server.SendAsync(c, new WebMessage(x)));
});

server.MessageReceived.OnChange(async args =>
{
    var (client, json) = args;
    
    var message = JObject.Parse(json);
    
    switch (message["Id"].Value<string>().ToLower())
    {
        case "runweb":
            var runWeb = JsonConvert.DeserializeObject<RunWebMessage>(json);
            runWeb
                .Web
                .Validate(node.Structures)
                .Pipe(x => node.Execute(x))
                .On(async x => await server.SendAsync(client, new WebResultMessage(x)))
                .OnError(Console.WriteLine);
            break;
        
        case "getweb":
            await server.SendAsync(args.Item1, new NodesStructureMessage(node.Structures, node.GenerateTypeDefinitions()));
            break;
    }
});

await server.MarkReady();
await recogniser.StartAsync();

Thread.Sleep(-1);
