using System.Collections.Immutable;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Vla.Abstractions;
using Vla.Abstractions.Connection;
using Vla.Addon;
using Vla.Addon.Services;
using Vla.Engine;
using Vla.Nodes;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Vla.Tests.Engine;

public class NodeEngine
{
	[Node]
	public class NumberConstantNode : Node
	{
		public override string Name => "Number constant";
		
		[NodeProperty] 
		public double Value { get; set; } = 100;
		
		public override async Task<ImmutableArray<NodeOutput>> Execute()
		{
			if (Value < 0)
				throw new ArgumentException("Value cannot be negative");

			return [Output("Value", Value)];
		}
	}
	
	[Node]
	public class MathAddNode : Node
	{
		public override string Name => "Add";
		
		public override async Task<ImmutableArray<NodeOutput>> Execute()
		{
			var a = Input("A", 1d);
			var b = Input("B", 2d);

			var result = a + b;

			return [Output("Result", result)];
		}
	}

	[Test]
	public async Task NodeEngine_Tick_ExecutesGraph()
	{
		var engine = CreateEngine();

		var properties = ImmutableDictionary<string, dynamic?>
			.Empty
			.Add("Value", 101);

		var numberConstantInstance = engine.CreateInstance<NumberConstantNode>(
			new InstanceOptions
			{
				Properties = properties
			});

		await engine.Tick();
		
		Assert.That(numberConstantInstance.Outputs["Value"], Is.EqualTo(101));
	}
	
	[Test]
	public async Task NodeEngine_Tick_FillsInDefaultInputValues()
	{
		var engine = CreateEngine(typeof(MathAddNode));
		
		var mathAddInstance = engine.CreateInstance<MathAddNode>();
		
		var results = await engine.Tick();
		
		Assert.That(mathAddInstance.Inputs["A"], Is.EqualTo(1));
		Assert.That(mathAddInstance.Inputs["B"], Is.EqualTo(2));
		Assert.That(results[0].Outputs[0].Value, Is.EqualTo(3));
	}
	
	[Test]
	public void NodeEngine_Tick_FillsInDefaultPropertyValues()
	{
		var engine = CreateEngine(typeof(NumberConstantNode));
		
		var numberConstantInstance = engine.CreateInstance<NumberConstantNode>();
		
		Assert.That(numberConstantInstance.Value, Is.EqualTo(100));
	}

	[Test]
	public async Task NodeEngine_Tick_HandlesExecutionException()
	{
		var engine = CreateEngine(typeof(NumberConstantNode));

		var properties = ImmutableDictionary<string, dynamic?>
			.Empty
			.Add("Value", -1);

		var numberConstantInstance = engine.CreateInstance<NumberConstantNode>(
			new InstanceOptions
			{
				Properties = properties
			});
		
		var results = await engine.Tick();
		
		Assert.That(results.Single().Exception, Is.Not.Null);
		Assert.That(results.Single().Exception!.Message, Is.EqualTo("Value cannot be negative"));
	}

	[Test]
	public void NodeEngine_Tick_DoesNotGetStuckInLoop()
	{
	
	}

	[Test]
	public void NodeEngine_Tick_ImplicitlyConvertsValues()
	{
	
	}
	
	[Test]
	public void NodeEngine_Tick_RunsDeterministicNodesOnce()
	{
		
	}
	
	[Test]
	public void NodeEngine_Tick_RerunsDeterministicIfChanged()
	{
	
	}
	
	private static Vla.Engine.NodeEngine CreateEngine(params Type[] nodes)
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

		var engine = ActivatorUtilities.CreateInstance<Vla.Engine.NodeEngine>(services);
		
		engine.RegisterNodes(nodes);

		return engine;
	}
}