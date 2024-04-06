using Vla.Addon;

namespace Vla.Addons.Math;

[Node]
public class IfElseNode : Node
{
	public override string Name { get; } = "If else";
	
	[OutgoingBranch("truthy", "Truthy")]
	public Branch TruthyBranch { get; } = new();
	
	[OutgoingBranch("falsy", "Falsy")]
	public Branch FalsyBranch { get; } = new();
	
	public override Task Execute()
	{
		var condition = Input("condition", "Condition", false);
		
		if(condition)
			TruthyBranch.Hits();
		else
			FalsyBranch.Hits();
		
		return Task.CompletedTask;
	}
}

[Node]
public class PrintNode : Node
{
	public override string Name => "Print";

	public override Task Execute()
	{
		var branch = InputBranch("branch", "Branch");
		var message = Input("message", "Message", string.Empty);
		
		if(branch)
			Console.WriteLine(message);

		return Task.CompletedTask;
	}
}

