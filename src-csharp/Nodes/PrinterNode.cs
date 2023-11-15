using Vla.Nodes.Attributes;

namespace Vla.Nodes;

[Node]
public class PrinterNode : INode
{
    public void Execute([NodeInput("Value")] double value)
    {
        Console.WriteLine(value);
    }
}