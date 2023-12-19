﻿using Vla.Abstractions;
using Vla.Abstractions.Attributes;
using Vla.Nodes;

namespace Vla.Extensions.Core.Math;

[Node("Power")]
[NodeCategory("Math")]
[NodeTags("Power", "Exponent", "Exponential", "Pow", "^")]
public class PowerMathNode : INode
{
    public string Name => "Power";

    public void Execute([NodeInput("Base")] double value, [NodeInput("Exponent")] double power, [NodeOutput("Result")] out double result)
    {
        result = System.Math.Pow(value, power);
    }
}