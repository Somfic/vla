using Vla.Addon;

namespace Vla.Addons.Math;

[Node]
public class BranchNode : Node
{
	public override string Name { get; } = "If else";
	
	public override Task Execute()
	{
		var branch = InputBranch("branch", "Branch");
		var condition = Input("condition", "Condition", false);
		
		OutputBranch("false", "On false", branch && !condition);
		OutputBranch("true", "On true", branch && condition);
		
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

