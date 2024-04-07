using FluentAssertions;
using Vla.Addon;

namespace Vla.Tests.Engine;

public class NodeEngine_Lazy
{
	[Node(NodePurity.Probabilistic)]
	class ConstantNumberNode : Node
	{
		public static int Number { get; set; } = 1;
		
		public override string Name => $"Constant number ({Number})";
		
		public override Task Execute()
		{
			Output("number", "Number", Number);
			
			return Task.CompletedTask;
		}
	}

	[Node]
	class IsEvenNode : Node
	{
		public override string Name => "Is even";
		
		public override Task Execute()
		{
			var number = Input("number", "Number", 0);
			Output("isEven", "Is even", number % 2 == 0);
			
			return Task.CompletedTask;
		}
	}
	
	[Node]
	class BooleanParity : Node
	{
		public override string Name => "Boolean parity";
		public override Task Execute()
		{
			var value = Input("value", "Value", false);
			Output("parity", "Parity", value ? "Odd" : "Even");

			return Task.CompletedTask;
		}
	}
	
	[Test]
	public async Task NodeEngine_Lazy_1()
	{
		var engine = NodeEngine.CreateEngine();
		
		var constantNumber = engine.CreateInstance<ConstantNumberNode>();
		var isEven = engine.CreateInstance<IsEvenNode>();
		var booleanParity = engine.CreateInstance<BooleanParity>();
		
		engine.CreateConnection(constantNumber, "number", isEven, "number");
		engine.CreateConnection(isEven, "isEven", booleanParity, "value");

		var result1 = await engine.Tick();

		var constantNumberResult1 = result1.First(x => x.Id == constantNumber.Id);
		var isEvenResult1 = result1.First(x => x.Id == isEven.Id);
		var booleanParityResult1 = result1.First(x => x.Id == booleanParity.Id);

		constantNumberResult1.Executed.Should().BeTrue();
		isEvenResult1.Executed.Should().BeTrue();
		booleanParityResult1.Executed.Should().BeTrue();
		
		// Since nothing changed since the last tick, the engine should return not execute any of the nodes
		
		var result2 = await engine.Tick();
		
		var constantNumberResult2 = result2.First(x => x.Id == constantNumber.Id);
		var isEvenResult2 = result2.First(x => x.Id == isEven.Id);
		var booleanParityResult2 = result2.First(x => x.Id == booleanParity.Id);
		
		// constantNumberResult2.Executed.Should().BeFalse();
		isEvenResult2.Executed.Should().BeFalse();
		booleanParityResult2.Executed.Should().BeFalse();
		
		// Though the outputted values should be the same
		
		constantNumberResult1.Outputs.Should().BeEquivalentTo(constantNumberResult2.Outputs);
		isEvenResult1.Outputs.Should().BeEquivalentTo(isEvenResult2.Outputs);
		booleanParityResult1.Outputs.Should().BeEquivalentTo(booleanParityResult2.Outputs);

		// If the number is changed, the engine should re-execute the nodes

		ConstantNumberNode.Number = 3;
		
		var result3 = await engine.Tick();
		 
		var constantNumberResult3 = result3.First(x => x.Id == constantNumber.Id);
		var isEvenResult3 = result3.First(x => x.Id == isEven.Id);
		var booleanParityResult3 = result3.First(x => x.Id == booleanParity.Id);
		
		constantNumberResult3.Executed.Should().BeTrue();
		isEvenResult3.Executed.Should().BeTrue();
		booleanParityResult3.Executed.Should().BeFalse();
	}
}