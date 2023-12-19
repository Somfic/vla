﻿using Vla.Abstractions;
using Vla.Abstractions.Attributes;
using Vla.Nodes;

namespace Vla.Extensions.Core.Math;

[Node("Absolute")]
[NodeCategory("Math")]
[NodeTags("Absolute", "Abs", "Magnitude")]
public class AbsoluteMathNode : INode
{
    public string Name => "Absolute";

    public void Execute([NodeInput("Value")] double value, [NodeOutput("Result")] out double result)
    {
        result = System.Math.Abs(value);
    }
}