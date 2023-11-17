using Vla.Nodes.Attributes;

namespace Vla.Nodes;

[Node]
public class PrinterNode : INode
{
    public void Execute([NodeInput("Value")] string value)
    {
        Console.WriteLine(value);
    }
}