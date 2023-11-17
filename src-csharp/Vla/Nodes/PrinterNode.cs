using Vla.Nodes.Attributes;

namespace Vla.Nodes;

[Node]
public class PrinterNode : INode
{
    public void Execute([NodeInput]NodeFlow flow, [NodeInput("Value")] string value = "")
    {
        if(flow.Triggered)
            Console.WriteLine(value);
    }
}

[Node]
public class ConditionalNode : INode
{
    public void Execute(
        [NodeInput("Condition")] bool condition, 
        [NodeOutput("True")] out NodeFlow onTrue,
        [NodeOutput("False")] out NodeFlow onFalse)
    {
        if (condition)
        {
            onTrue = NodeFlow.Execute();
            onFalse = NodeFlow.Ignore();
        }
        else
        {
            onTrue = NodeFlow.Ignore();
            onFalse = NodeFlow.Execute();
        }
    }
}

public readonly struct NodeFlow : INode
{
    public bool Triggered { get; init; }
    
    public static NodeFlow Execute()
    {
        return new NodeFlow { Triggered = true };
    }
    
    public static NodeFlow Ignore()
    {
        return new NodeFlow { Triggered = false };
    }
}