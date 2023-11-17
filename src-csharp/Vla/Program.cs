using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Somfic.Common;
using Vla.Nodes;
using Vla.Nodes.Connection;
using Vla.Nodes.Instance;
using Vla.Nodes.Structure;
using Vla.Nodes.Web;
using Vla.Nodes.Web.Result;
using Vla.Server;
using Vla.Server.Messages;
using Vla.Voice;
using WatsonWebsocket;

var host = Host.CreateDefaultBuilder()
    .ConfigureServices(s =>
    {
        s.AddSingleton<RecogniserService>();
        s.AddSingleton<WebsocketService>();
        s.AddSingleton<NodeService>();
    })
    .Build();

// var recogniser = host.Services.GetRequiredService<RecogniserService>();
var server = host.Services.GetRequiredService<WebsocketService>();
var node = host.Services.GetRequiredService<NodeService>();

node.Register<MathNode>()
    .Register<NumberConstantNode>()
    .Register<PrinterNode>();

var constantInstance1 = new NodeInstance()
    .From<NumberConstantNode>()
    .WithProperty("Value", 2m);

var constantInstance2 = new NodeInstance()
    .From<NumberConstantNode>()
    .WithProperty("Value", 3m);

var mathInstance = new NodeInstance()
    .From<MathNode>();

var printInstance = new NodeInstance()
    .From<PrinterNode>();

var constant1ToMathA = new NodeConnection()
    .From(constantInstance1, "value")
    .To(mathInstance, "a");

var constant2ToMathB = new NodeConnection()
    .From(constantInstance2, "value")
    .To(mathInstance, "b");

var mathToPrint = new NodeConnection()
    .From(mathInstance, "result")
    .To(printInstance, "value");

var instances = new[] { constantInstance1, constantInstance2, mathInstance, printInstance };
var connections = new[] { constant1ToMathA, constant2ToMathB, mathToPrint };
var web = new Web()
    .WithInstances(instances)
    .WithConnections()
    .Validate(node.Structures);

server.ClientConnected.OnChange(async c =>
{
    await server.Send(c, new NodesStructureMessage(node.Structures, node.GenerateTypeDefinitions()));
    web.On(async x => await server.Send(c, new WebMessage(x)));
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
                .On(async x => await server.Send(client, new WebResultMessage(x)))
                .OnError(Console.WriteLine);
            break;
    }
    
});

// recogniser.Recognised.OnChange(async s =>
// {
//     await server.Broadcast(new RecogniserRecognisedMessage(s));
// });
//
// recogniser.PartlyRecognised.OnChange(async s =>
// {
//     await server.Broadcast(new RecogniserRecognisedPartialMessage(s));
// });

// var constantInstance1 = new NodeInstance()
//     .From<NumberConstantNode>()
//     .WithProperty("Value", "2");
//
// var constantInstance2 = new NodeInstance()
//     .From<NumberConstantNode>()
//     .WithProperty("Value", "2");
//
// var mathInstance = new NodeInstance()
//     .From<MathNode>();
//
// var printInstance = new NodeInstance()
//     .From<PrinterNode>();
//
// var constant1ToMathA = new NodeConnection()
//     .From(constantInstance1, "value")
//     .To(mathInstance, "a");
//
// var constant2ToMathB = new NodeConnection()
//     .From(constantInstance2, "value")
//     .To(mathInstance, "b");
//
// var mathToPrint = new NodeConnection()
//     .From(mathInstance, "result")
//     .To(printInstance, "value");
//
// node.Execute(new[] { constantInstance1, constantInstance2, mathInstance, printInstance }, new[] { constant1ToMathA, constant2ToMathB, mathToPrint });

await server.Start();

Thread.Sleep(-1);
