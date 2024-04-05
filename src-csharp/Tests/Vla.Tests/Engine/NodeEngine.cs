using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Vla.Abstractions;
using Vla.Addon;
using Vla.Addon.Services;
using Vla.Services;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Vla.Tests.Engine;

public class NodeEngine
{
	[Test]
	public async Task NodeEngine_Tick_ExecutesGraph()
	{
		var engine = CreateEngine();

		var mathAddInstance = engine.CreateInstance<MathAddNode>();

		var numberConstantInstance = engine.CreateInstance<NumberConstantNode>(
			new NodeInstance
			{
				Properties = [new NamedValue("Value", "Value", 101)]
			});

		engine.CreateConnection(numberConstantInstance, "result", mathAddInstance, "a");

		var results = await engine.Tick();

		Assert.That(results[0].Executed, Is.True);
		Assert.That(results[0].Exception, Is.Null);
		Assert.That(results[0].GetOutput("result").Value, Is.EqualTo(101));
		Assert.That(results[1].GetInput("a").Value, Is.EqualTo(101));
	}

	[Test]
	public async Task NodeEngine_Tick_FillsInDefaultInputValues()
	{
		var engine = CreateEngine();

		var mathAddInstance = engine.CreateInstance<MathAddNode>();

		var results = await engine.Tick();

		Assert.That(results[0].Executed, Is.True);
		Assert.That(results[0].Exception, Is.Null);
		Assert.That(results[0].GetInput("a").Value, Is.EqualTo(1));
		Assert.That(results[0].GetInput("b").Value, Is.EqualTo(2));
		Assert.That(results[0].GetOutput("result").Value, Is.EqualTo(3));
	}

	[Test]
	public void NodeEngine_CreateInstance_FillsInDefaultPropertyValues()
	{
		var engine = CreateEngine();

		var numberConstantInstance = engine.CreateInstance<NumberConstantNode>();

		Assert.That(numberConstantInstance.Value, Is.EqualTo(100));
	}

	[Test]
	public async Task NodeEngine_Tick_HandlesExecutionException()
	{
		var engine = CreateEngine();

		var numberConstantInstance = engine.CreateInstance<NumberConstantNode>(
			new NodeInstance
			{
				Properties = [new NamedValue("Value", "Value", -1)]
			});

		var results = await engine.Tick();

		Assert.That(results[0].Executed, Is.True);
		Assert.That(results[0].Exception, Is.Not.Null);
		Assert.That(results[0].Exception!.Message, Is.EqualTo("Value cannot be negative"));
	}

	[Test]
	public async Task NodeEngine_Tick_DoesNotGetStuckInLoop()
	{
		var engine = CreateEngine();

		var mathAddInstance = engine.CreateInstance<MathAddNode>();

		engine.CreateConnection(new NodeConnection(mathAddInstance.Id.ToString(), "result",
			mathAddInstance.Id.ToString(), "a"));

		for (var i = 0; i < 10000; i++)
		{
			var results = await engine.Tick();

			Assert.That(results[0].Executed, Is.True);
			Assert.That(results[0].Exception, Is.Null);
		}
	}

	[Test]
	public async Task NodeEngine_Tick_ImplicitlyConvertsValues()
	{
		var engine = CreateEngine();

		var textConstantInstance = engine.CreateInstance<TextConstantNode>();
		var mathAddInstance = engine.CreateInstance<MathAddNode>();

		engine.CreateConnection(textConstantInstance, "result", mathAddInstance, "a");

		var results = await engine.Tick();

		Assert.That(results[0].Executed, Is.True);
		Assert.That(results[0].Exception, Is.Null);
		Assert.That(results[1].Executed, Is.True);
		Assert.That(results[1].Exception, Is.Null);

		Assert.That(results[1].GetInput("a").Value, Is.EqualTo(12));
	}

	[Test]
	[Ignore("To be implemented")]
	public async Task NodeEngine_Tick_RunsDeterministicNodesOnce()
	{
		var engine = CreateEngine();

		engine.CreateInstance<MathAddNode>();

		var results = await engine.Tick();

		Assert.That(results[0].Executed, Is.True);

		results = await engine.Tick();

		Assert.That(results[0].Executed, Is.False);
	}

	[Test]
	public void NodeEngine_Tick_RerunsProbabilisticNodesEveryTick()
	{
		var engine = CreateEngine();

		engine.CreateInstance<CurrentTimeNode>();

		var result = engine.Tick().Result;

		Assert.That(result[0].Executed, Is.True);

		result = engine.Tick().Result;

		Assert.That(result[0].Executed, Is.True);
	}

	[Test]
	[Ignore("To be implemented")]
	public void NodeEngine_CreateInstance_ThrowsIfNodeNotRegistered()
	{
		var engine = CreateEngine();

		Assert.Throws<InvalidOperationException>(() => engine.CreateInstance<MathAddNode>());
	}

	[Test]
	public async Task NodeEngine_Web_SavesGraph()
	{
		var engine = CreateEngine();

		var mathAddInstance = engine.CreateInstance<MathAddNode>();

		engine.CreateConnection(mathAddInstance, "result", mathAddInstance, "a");

		await engine.Tick();

		Assert.That(engine.Instances.Count, Is.EqualTo(1));
		Assert.That(engine.Connections.Count, Is.EqualTo(1));
		Assert.That(engine.Instances[0].Outputs["result"], Is.EqualTo(3));

		var state = engine.SaveWeb();

		var engine2 = CreateEngine();
		engine2.LoadWeb(state);

		Assert.That(engine2.Instances.Count, Is.EqualTo(1));
		Assert.That(engine2.Connections.Count, Is.EqualTo(1));
		Assert.That(engine2.Instances[0].Outputs["result"], Is.EqualTo(3));

		await engine2.Tick();

		Assert.That(engine2.Instances[0].Outputs["result"], Is.EqualTo(5));
	}

	[Test]
	public async Task NodeEngine_Tick_FillsInLabels()
	{
		var engine = CreateEngine();

		var mathAddInstance = engine.CreateInstance<MathAddNode>();

		var results = await engine.Tick();

		Assert.That(results[0].Executed, Is.True);
		Assert.That(results[0].Exception, Is.Null);
		Assert.That(results[0].GetInput("a").Label, Is.EqualTo("Value"));
		Assert.That(results[0].GetInput("b").Label, Is.EqualTo("Value"));
		Assert.That(results[0].GetOutput("result").Label, Is.EqualTo("Result"));
	}
	
	[Test]
	public async Task NodeEngine_Tick_RespectsBranches()
	{
		var engine = CreateEngine();

		var booleanNode = engine.CreateInstance<BooleanNode>();
		var ifElseNode = engine.CreateInstance<IfElse>();
		
		var truthyBranch = engine.CreateInstance<DummyBranchNode>();
		truthyBranch.CustomName = "Truthy branch #1";
		
		var truthyExtendedBranch = engine.CreateInstance<DummyBranchNode>();
		truthyExtendedBranch.CustomName = "Truthy branch #2";
		
		var falsyBranch = engine.CreateInstance<DummyBranchNode>();
		falsyBranch.CustomName = "Falsy branch #1";
		
		var falsyExtendedBranch = engine.CreateInstance<DummyBranchNode>();
		falsyExtendedBranch.CustomName = "Falsy branch #2";
		
		engine.CreateConnection(booleanNode, "value", ifElseNode, "condition");
		engine.CreateConnection(ifElseNode, "true", truthyBranch, "branch");
		engine.CreateConnection(ifElseNode, "false", falsyBranch, "branch");
		engine.CreateConnection(truthyBranch, "branch", truthyExtendedBranch, "branch");
		engine.CreateConnection(falsyBranch, "branch", falsyExtendedBranch, "branch");
		
		BooleanNode.Value = true;
		
		var results = await engine.Tick();
		results = await engine.Tick();

		
		
		Assert.That(results.First(x => x.Id == truthyBranch.Id).Executed, Is.True);
		Assert.That(results.First(x => x.Id == truthyExtendedBranch.Id).Executed, Is.True);
		Assert.That(results.First(x => x.Id == falsyBranch.Id).Executed, Is.False);
		Assert.That(results.First(x => x.Id == falsyExtendedBranch.Id).Executed, Is.False);
		
		BooleanNode.Value = false;
		
		results = await engine.Tick();
		results = await engine.Tick();
		
		Assert.That(results.First(x => x.Id == truthyBranch.Id).Executed, Is.False);
		Assert.That(results.First(x => x.Id == truthyExtendedBranch.Id).Executed, Is.False);
		Assert.That(results.First(x => x.Id == falsyBranch.Id).Executed, Is.True);
		Assert.That(results.First(x => x.Id == falsyExtendedBranch.Id).Executed, Is.True);
	}

	private static Vla.Engine.NodeEngine CreateEngine()
	{
		var services = Host.CreateDefaultBuilder()
			.ConfigureServices(s =>
			{
				s.AddSingleton<IVariableManager, VariableManager>();
				s.AddSingleton<AddonService>();
			})
			.ConfigureLogging(l =>
			{
				l.AddConsole();
				l.SetMinimumLevel(LogLevel.Warning);
			})
			.Build()
			.Services;

		var engine = ActivatorUtilities.CreateInstance<Vla.Engine.NodeEngine>(services);

		return engine;
	}

	[Node]
	public class TextConstantNode : Node
	{
		public override string Name { get; }

		public override Task Execute()
		{
			Output("result", "Result", "12");
			return Task.CompletedTask;
		}
	}

	[Node]
	public class NumberConstantNode : Node
	{
		public override string Name => "Number constant";

		[NodeProperty]
		public double Value { get; set; } = 100;

		public override Task Execute()
		{
			if (Value < 0)
				throw new ArgumentException("Value cannot be negative");

			Output("result", "Result", Value);
			return Task.CompletedTask;
		}
	}

	[Node]
	public class MathAddNode : Node
	{
		public override string Name => "Add";

		public override Task Execute()
		{
			var a = Input("a", "Value", 1d);
			var b = Input("b", "Value", 2d);

			var result = a + b;

			Output("result", "Result", result);

			return Task.CompletedTask;
		}
	}

	[Node]
	public class BooleanNode : Node
	{
		public override string Name => "Boolean";
		
		public static bool Value { get; set; } = false;
		
		public override Task Execute()
		{
			Output("value", "Value", Value);
			return Task.CompletedTask;
		}
	}

	[Node(NodePurity.Probabilistic)]
	public class CurrentTimeNode : Node
	{
		public override string Name { get; }

		public override Task Execute()
		{
			OutputBranch("branch", "", true);
			Output("hour", "Hour", DateTime.Now.Hour);
			Output("minute", "Minute", DateTime.Now.Minute);
			Output("second", "Second", DateTime.Now.Second);
			return Task.CompletedTask;
		}
	}

	[Node]
	public class DummyBranchNode : Node
	{
		public string CustomName { get; set; } = "Boolean";
		
		public override string Name => CustomName;
		
		public override Task Execute()
		{
			var branch = InputBranch("branch", "Branch");
			OutputBranch("branch", "Branch", branch);
			return Task.CompletedTask;
		}
	
	}

	[Node]
	public class IfElse : Node
	{
		public override string Name => "If else";
		
		public override Task Execute()
		{
			var branch = InputBranch("branch", "Branch");
			var condition = Input("condition", "Condition", false);
		
			OutputBranch("false", "On false", branch && !condition);
			OutputBranch("true", "On true", branch && condition);
		
			return Task.CompletedTask;
		}
	}
}