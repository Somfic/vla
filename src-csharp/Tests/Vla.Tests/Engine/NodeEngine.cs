using System.Collections.Immutable;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Somfic.Common;
using Vla.Abstractions;
using Vla.Abstractions.Connection;
using Vla.Abstractions.Instance;
using Vla.Abstractions.Structure;
using Vla.Addon;
using Vla.Addon.Core.Variables;
using Vla.Addon.Services;
using Vla.Nodes;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Vla.Tests.Engine;

public class NodeEngine
{
	[Node(Purity.Deterministic)]
	public class NumberConstantNode : INode
	{
		public string Name => "Number constant";

		[NodeProperty] public double Value { get; set; } = 100;
		
		public void Execute([NodeOutput] out double result)
		{
			if (Value < 0)
				throw new ArgumentException("Value cannot be negative");
			
			result = Value;
		}
	}
	
	[Node(Purity.Deterministic)]
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

		ImmutableArray<NodeStructure> structures = [addStructure, constantStructure];
		ImmutableArray<NodeInstance> instances = [constantInstance1, constantInstance2, addInstance];
		ImmutableArray<NodeConnection> connections = [constantToAConnection, constantToBConnection];

		var engine = CreateEngine(structures, instances, connections);

		engine.Tick();

		Assert.That(engine.Values[$"{constantInstance1.Id}.result"], Is.EqualTo(12.5));
		Assert.That(engine.Values[$"{constantInstance2.Id}.result"], Is.EqualTo(13));
		Assert.That(engine.Values[$"{addInstance.Id}.result"], Is.EqualTo(25));
	}
	
	[Test]
	public void NodeEngine_Tick_FillsInDefaultStructureInputValues()
	{
		var addStructure = NodeExtensions.ToStructure<MathAddNode>().Expect();
		var constantStructure = NodeExtensions.ToStructure<NumberConstantNode>().Expect();

		var constantInstance = new NodeInstance().From(constantStructure);
		
		var addInstance = new NodeInstance().From(addStructure);

		var constantToAConnection = new NodeConnection()
			.WithSource(constantInstance, "result")
			.WithTarget(addInstance, "a");

		ImmutableArray<NodeStructure> structures = [addStructure, constantStructure];
		ImmutableArray<NodeInstance> instances = [constantInstance, addInstance];
		ImmutableArray<NodeConnection> connections = [constantToAConnection];

		var engine = CreateEngine(structures, instances, connections);

		engine.Tick();

		Assert.That(engine.Values[$"{addInstance.Id}.b"], Is.EqualTo(1));
	}
	
	[Test]
	public void NodeEngine_Tick_FillsInDefaultInstanceInputValues()
	{
		var addStructure = NodeExtensions.ToStructure<MathAddNode>().Expect();
		var constantStructure = NodeExtensions.ToStructure<NumberConstantNode>().Expect();

		var constantInstance = new NodeInstance().From(constantStructure)
			.WithProperty("Value", 12.5);
		
		var addInstance = new NodeInstance().From(addStructure)
			.WithInput("b", 9999);

		var constantToAConnection = new NodeConnection()
			.WithSource(constantInstance, "result")
			.WithTarget(addInstance, "a");

		ImmutableArray<NodeStructure> structures = [addStructure, constantStructure];
		ImmutableArray<NodeInstance> instances = [constantInstance, addInstance];
		ImmutableArray<NodeConnection> connections = [constantToAConnection];

		var engine = CreateEngine(structures, instances, connections);

		engine.Tick();

		Assert.That(engine.Values[$"{addInstance.Id}.b"], Is.EqualTo(9999));
	}
	
	[Test]
	public void NodeEngine_Tick_FillsInDefaultStructurePropertyValues()
	{
		var addStructure = NodeExtensions.ToStructure<MathAddNode>().Expect();
		var constantStructure = NodeExtensions.ToStructure<NumberConstantNode>().Expect();

		var constantInstance = new NodeInstance().From(constantStructure);
		
		var addInstance = new NodeInstance().From(addStructure);

		var constantToAConnection = new NodeConnection()
			.WithSource(constantInstance, "result")
			.WithTarget(addInstance, "a");

		ImmutableArray<NodeStructure> structures = [addStructure, constantStructure];
		ImmutableArray<NodeInstance> instances = [constantInstance, addInstance];
		ImmutableArray<NodeConnection> connections = [constantToAConnection];

		var engine = CreateEngine(structures, instances, connections);

		engine.Tick();

		Assert.That(engine.Values[$"{constantInstance.Id}.result"], Is.EqualTo(100));
	}
	
	[Test]
	public void NodeEngine_Tick_FillsInDefaultInstancePropertyValues()
	{
		var addStructure = NodeExtensions.ToStructure<MathAddNode>().Expect();
		var constantStructure = NodeExtensions.ToStructure<NumberConstantNode>().Expect();

		var constantInstance = new NodeInstance().From(constantStructure).WithProperty("Value", 12222);
		
		var addInstance = new NodeInstance().From(addStructure);

		var constantToAConnection = new NodeConnection()
			.WithSource(constantInstance, "result")
			.WithTarget(addInstance, "a");

		ImmutableArray<NodeStructure> structures = [addStructure, constantStructure];
		ImmutableArray<NodeInstance> instances = [constantInstance, addInstance];
		ImmutableArray<NodeConnection> connections = [constantToAConnection];

		var engine = CreateEngine(structures, instances, connections);

		engine.Tick();

		Assert.That(engine.Values[$"{constantInstance.Id}.result"], Is.EqualTo(12222));
	}

	[Test]
	public void NodeEngine_Tick_HandlesExecutionException()
	{
		var addStructure = NodeExtensions.ToStructure<MathAddNode>().Expect();
		var constantStructure = NodeExtensions.ToStructure<NumberConstantNode>().Expect();

		var constantInstance = new NodeInstance().From(constantStructure)
			.WithProperty("Value", -1);
		
		var addInstance = new NodeInstance().From(addStructure);

		var constantToAConnection = new NodeConnection()
			.WithSource(constantInstance, "result")
			.WithTarget(addInstance, "a");

		ImmutableArray<NodeStructure> structures = [addStructure, constantStructure];
		ImmutableArray<NodeInstance> instances = [constantInstance, addInstance];
		ImmutableArray<NodeConnection> connections = [constantToAConnection];

		var engine = CreateEngine(structures, instances, connections);

		engine.Tick();

		Assert.That(engine.Values[$"{constantInstance.Id}.result"], Is.EqualTo(0));
	}

	[Test]
	public void NodeEngine_Tick_DoesNotGetStuckInLoop()
	{
		var addStructure = NodeExtensions.ToStructure<MathAddNode>().Expect();
		var addInstance = new NodeInstance().From(addStructure);
		var connection = new NodeConnection()
			.WithSource(addInstance, "result")
			.WithTarget(addInstance, "a");
		
		ImmutableArray<NodeStructure> structures = [addStructure];
		ImmutableArray<NodeInstance> instances = [addInstance];
		ImmutableArray<NodeConnection> connections = [connection];
		
		var engine = CreateEngine(structures, instances, connections);

		for (int i = 0; i < 1000; i++)
		{
			engine.Tick();
			Assert.That(engine.Values[$"{addInstance.Id}.result"], Is.EqualTo(i + 1));
		}
	}

	[Test]
	public void NodeEngine_Tick_ImplicitlyConvertsValues()
	{
		var setStringStructure = NodeExtensions.ToStructure<SetStringVariable>().Expect();
		var getStringStructure = NodeExtensions.ToStructure<GetStringVariable>().Expect();
		
		var setStringInstance = new NodeInstance().From(setStringStructure).WithInput("value", "Hello, world!");
		var getStringInstance = new NodeInstance().From(getStringStructure);
		
		var setStringToGetConnection = new NodeConnection()
			.WithSource(setStringInstance, "result")
			.WithTarget(getStringInstance, "value");
		
		ImmutableArray<NodeStructure> structures = [setStringStructure, getStringStructure];
		ImmutableArray<NodeInstance> instances = [setStringInstance, getStringInstance];
		ImmutableArray<NodeConnection> connections = [setStringToGetConnection];
		
		var engine = CreateEngine(structures, instances, connections);
		
		engine.Tick();
		
		Assert.That(engine.Values[$"{setStringInstance.Id}.value"], Is.EqualTo("Hello, world!"));
		Assert.That(engine.Values[$"{getStringInstance.Id}.value"], Is.EqualTo("Hello, world!"));
	}
	
	[Test]
	public void NodeEngine_Tick_RunsDeterministicNodesOnce()
	{
		var addStructure = NodeExtensions.ToStructure<MathAddNode>().Expect();
		var constantStructure = NodeExtensions.ToStructure<NumberConstantNode>().Expect();

		var constantInstance = new NodeInstance().From(constantStructure).WithProperty("Value", 12.5);
		var addInstance = new NodeInstance().From(addStructure);

		var constantToAConnection = new NodeConnection()
			.WithSource(constantInstance, "result")
			.WithTarget(addInstance, "a");

		ImmutableArray<NodeStructure> structures = [addStructure, constantStructure];
		ImmutableArray<NodeInstance> instances = [constantInstance, addInstance];
		ImmutableArray<NodeConnection> connections = [constantToAConnection];

		var engine = CreateEngine(structures, instances, connections);

		var results1 = engine.Tick();

		Assert.That(results1, Has.Length.EqualTo(2));
		Assert.That(results1[0].WasExecuted, Is.True);
		Assert.That(results1[1].WasExecuted, Is.True);

		var results2 = engine.Tick();
		
		Assert.That(results2, Has.Length.EqualTo(2));
		Assert.That(results2[0].WasExecuted, Is.False);
		Assert.That(results2[1].Outputs.SequenceEqual(results1[1].Outputs), Is.True);
		Assert.That(results2[1].WasExecuted, Is.False);
		Assert.That(results2[1].Outputs.SequenceEqual(results1[1].Outputs), Is.True);
	}
	
	[Test]
	public void NodeEngine_Tick_RerunsDeterministicIfChanged()
	{
		var addStructure = NodeExtensions.ToStructure<MathAddNode>().Expect();
		var constantStructure = NodeExtensions.ToStructure<NumberConstantNode>().Expect();

		var constantInstance = new NodeInstance().From(constantStructure).WithProperty("Value", 12.5);
		var addInstance = new NodeInstance().From(addStructure);

		var constantToAConnection = new NodeConnection()
			.WithSource(constantInstance, "result")
			.WithTarget(addInstance, "a");

		ImmutableArray<NodeStructure> structures = [addStructure, constantStructure];
		ImmutableArray<NodeInstance> instances = [constantInstance, addInstance];
		ImmutableArray<NodeConnection> connections = [constantToAConnection];

		var engine = CreateEngine(structures, instances, connections);

		var results1 = engine.Tick();

		Assert.That(results1, Has.Length.EqualTo(2));
		Assert.That(results1[0].WasExecuted, Is.True);
		Assert.That(results1[1].WasExecuted, Is.True);

		constantInstance = constantInstance.WithProperty("Value", 13);
		instances = [constantInstance, addInstance];
		engine.SetGraph(instances, connections);

		var results2 = engine.Tick();
		
		Assert.That(results2, Has.Length.EqualTo(2));
		Assert.That(results2[0].WasExecuted, Is.True);
		Assert.That(results2[1].WasExecuted, Is.True);
	}
	
	private static Vla.Engine.NodeEngine CreateEngine(ImmutableArray<NodeStructure> structures, ImmutableArray<NodeInstance> instances, ImmutableArray<NodeConnection> connections)
	{
		var services = Host.CreateDefaultBuilder()
			.ConfigureServices(s =>
			{
				s.AddSingleton<IVariableManager, VariableManager>();
			})
			.ConfigureLogging(l =>
			{
				l.AddConsole();
				l.SetMinimumLevel(LogLevel.Trace);
			})
			.Build()
			.Services;
		
		return ActivatorUtilities.CreateInstance<Vla.Engine.NodeEngine>(services)
			.SetStructures(structures)
			.SetGraph(instances, connections);
	}
}