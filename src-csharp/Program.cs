using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Somfic.Common;
using Vla.Nodes;
using Vla.Nodes.Connection;
using Vla.Nodes.Instance;
using Vla.Nodes.Structure;
using Vla.Nodes.Web;
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

// register nodes through plugins

server.ClientConnected.OnChange(async c =>
{
    await server.Send(c, new NodesStructureMessage(node.Structures));
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
