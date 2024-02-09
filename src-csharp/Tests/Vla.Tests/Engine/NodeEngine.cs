using System.Collections.Immutable;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
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
	public class TextConstantNode : Node
	{
		public override string Name { get; }
		public override async Task<ImmutableArray<NodeOutput>> Execute()
		{
			return [Output("Value", "12")];
		}
	}
	
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
			Console.WriteLine("Hello");
			
			var a = Input("A", 1d);
			var b = Input("B", 2d);

			Console.WriteLine($"NODE: inputs {JsonConvert.SerializeObject(Inputs)}");
            
			var result = a + b;

			Console.WriteLine($"NODE: {result} = {a} + {b}");
			
			return [Output("Result", result)];
		}
	}

	[Test]
	public async Task NodeEngine_Tick_ExecutesGraph()
	{
		var engine = CreateEngine();
		
		var mathAddInstance = engine.CreateInstance<MathAddNode>();
		
		var numberConstantInstance = engine.CreateInstance<NumberConstantNode>(
			new InstanceOptions
			{
				Properties = ImmutableDictionary<string, dynamic?>.Empty.Add("Value", 101)
			});
		
		engine.CreateConnection(numberConstantInstance, "Value", mathAddInstance, "A");

		await engine.Tick();
		
		Assert.That(numberConstantInstance.Outputs["Value"], Is.EqualTo(101));
		Assert.That(mathAddInstance.Inputs["A"], Is.EqualTo(101));
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
	public void NodeEngine_CreateInstance_FillsInDefaultPropertyValues()
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
	public async Task NodeEngine_Tick_DoesNotGetStuckInLoop()
	{
		var engine = CreateEngine(typeof(MathAddNode));
		
		var mathAddInstance = engine.CreateInstance<MathAddNode>();
		
		engine.CreateConnection(new NodeConnection(mathAddInstance, "Result", mathAddInstance, "A"));

		var result = await engine.Tick();
		
		Assert.That(result.Single().Exception, Is.Null);
		Assert.That(mathAddInstance.Outputs["Result"], Is.EqualTo(3));
		
		result = await engine.Tick();
		
		Assert.That(result.Single().Exception, Is.Null);
		Assert.That(mathAddInstance.Outputs["Result"], Is.EqualTo(5));
	}

	[Test]
	public async Task NodeEngine_Tick_ImplicitlyConvertsValues()
	{
		var engine = CreateEngine();
		
		var textConstantInstance = engine.CreateInstance<TextConstantNode>();
		var mathAddInstance = engine.CreateInstance<MathAddNode>();
		
		engine.CreateConnection(textConstantInstance, "Value", mathAddInstance, "A");

		var results = await engine.Tick();
		
		foreach (var nodeExecutionResult in results)
		{
			Assert.That(nodeExecutionResult.Exception, Is.Null);
		}
		Assert.That(mathAddInstance.Inputs["A"], Is.EqualTo(12));
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