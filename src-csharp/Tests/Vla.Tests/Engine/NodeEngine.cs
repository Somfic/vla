using System.Collections.Immutable;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Somfic.Common;
using Vla.Abstractions.Attributes;
using Vla.Engine;
using Vla.Nodes;
using Vla.Nodes.Connection;
using Vla.Nodes.Instance;

namespace Vla.Tests.Engine;

public class NodeEngine
{
	[Node("Number constant")]
	public class NumberConstantNode : INode
	{
		public string Name => "Number constant";
		
		[NodeProperty]
		public int Value { get; set; }
		
		public void Execute([NodeOutput] out int result)
		{
			result = Value;
		}
	}
	
	[Node("Math add")]
	public class MathAddNode : INode
	{
		public string Name => "Add";
		
		public void Execute([NodeInput] int a, [NodeInput] int b, [NodeOutput] out int result)
		{
			result = a + b;
		}
	}

	[Test]
	public void NodeEngine_Tick_ExecutesGraph()
	{
		var addStructure = NodeExtensions.ToStructure<MathAddNode>().Expect();
		var constantStructure = NodeExtensions.ToStructure<NumberConstantNode>().Expect();

		var constantInstance1 = new NodeInstance().From(constantStructure).WithProperty("Value", 12);
		var constantInstance2 = new NodeInstance().From(constantStructure).WithProperty("Value", 13);

		var addInstance = new NodeInstance().From(addStructure);

		var constantToAConnection = new NodeConnection()
			.WithSource(constantInstance1, "result")
			.WithTarget(addInstance, "a");

		var constantToBConnection = new NodeConnection()
			.WithSource(constantInstance2, "result")
			.WithTarget(addInstance, "b");

		ImmutableArray<NodeInstance> instances = [constantInstance1, constantInstance2, addInstance];
		ImmutableArray<NodeConnection> connections = [constantToAConnection, constantToBConnection];

		var services = new ServiceCollection().BuildServiceProvider();
		
		var engine = new Vla.Engine.NodeEngine(services)
			.SetStructures(addStructure, constantStructure)
			.SetGraph(instances, connections);

		engine.Tick();

		Assert.That(engine.Values[$"{constantInstance1.Id}.result"], Is.EqualTo(12));
		Assert.That(engine.Values[$"{constantInstance2.Id}.result"], Is.EqualTo(13));
		Assert.That(engine.Values[$"{addInstance.Id}.result"], Is.EqualTo(25));
	}
}