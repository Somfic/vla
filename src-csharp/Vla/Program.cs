using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Somfic.Common;
using Vla.Input;
using Vla.Nodes;
using Vla.Nodes.Constant;
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
    server.BroadcastAsync(new ProgressMessage(e.Percentage, e.Label)).GetAwaiter().GetResult();
});

recogniser.Recognised.OnChange(e =>
{
    var processedText = processor.Process(e.Text);
    server.BroadcastAsync(new RecogniserRecognisedMessage(processedText)).GetAwaiter().GetResult();
});

recogniser.PartlyRecognised.OnChange(e =>
{
    server.BroadcastAsync(new RecogniserRecognisedPartialMessage(e)).GetAwaiter().GetResult();
});

node.Register(typeof(BooleanConstantNode).Assembly);

var web = new Web()
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
