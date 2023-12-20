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

		[NodeProperty] public double Value { get; set; } = 100;
		
		public void Execute([NodeOutput] out double result)
		{
			result = Value;
		}
	}
	
	[Node("Math add")]
	public class MathAddNode : INode
	{
		public string Name => "Add";
		
		public void Execute([NodeOutput] out int result, [NodeInput] int a, [NodeInput] int b = 1)
		{
			result = a + b;
		}
	}

	[Test]
	public void NodeEngine_Tick_ExecutesGraph()
	{
		var addStructure = NodeExtensions.ToStructure<MathAddNode>().Expect();
		var constantStructure = NodeExtensions.ToStructure<NumberConstantNode>().Expect();

		var constantInstance1 = new NodeInstance().From(constantStructure).WithProperty("Value", 12.5);
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

		Assert.That(engine.Values[$"{constantInstance1.Id}.result"], Is.EqualTo(12.5));
		Assert.That(engine.Values[$"{constantInstance2.Id}.result"], Is.EqualTo(13));
		Assert.That(engine.Values[$"{addInstance.Id}.result"], Is.EqualTo(25));
	}
	
	[Test]
	public void NodeEngine_Tick_FillsInDefaultInputValues()
	{
		var addStructure = NodeExtensions.ToStructure<MathAddNode>().Expect();
		var constantStructure = NodeExtensions.ToStructure<NumberConstantNode>().Expect();

		var constantInstance = new NodeInstance().From(constantStructure).WithProperty("Value", 12.5);
		
		var addInstance = new NodeInstance().From(addStructure);

		var constantToAConnection = new NodeConnection()
			.WithSource(constantInstance, "result")
			.WithTarget(addInstance, "a");

		ImmutableArray<NodeInstance> instances = [constantInstance, addInstance];
		ImmutableArray<NodeConnection> connections = [constantToAConnection];

		var services = new ServiceCollection().BuildServiceProvider();
		
		var engine = new Vla.Engine.NodeEngine(services)
			.SetStructures(addStructure, constantStructure)
			.SetGraph(instances, connections);

		engine.Tick();

		Assert.That(engine.Values[$"{addInstance.Id}.b"], Is.EqualTo(1));
	}
	
	[Test]
	public void NodeEngine_Tick_FillsInDefaultPropertyValues()
	{
		var addStructure = NodeExtensions.ToStructure<MathAddNode>().Expect();
		var constantStructure = NodeExtensions.ToStructure<NumberConstantNode>().Expect();

		var constantInstance = new NodeInstance().From(constantStructure);
		
		var addInstance = new NodeInstance().From(addStructure);

		var constantToAConnection = new NodeConnection()
			.WithSource(constantInstance, "result")
			.WithTarget(addInstance, "a");

		ImmutableArray<NodeInstance> instances = [constantInstance, addInstance];
		ImmutableArray<NodeConnection> connections = [constantToAConnection];

		var services = new ServiceCollection().BuildServiceProvider();
		
		var engine = new Vla.Engine.NodeEngine(services)
			.SetStructures(addStructure, constantStructure)
			.SetGraph(instances, connections);

		engine.Tick();

		Assert.That(engine.Values[$"{constantInstance.Id}.result"], Is.EqualTo(100));
	}
}