﻿using Vla.Abstractions;
using Vla.Abstractions.Attributes;

namespace Vla.Extensions.Core.Math;

[Node("Exponent")]
[NodeCategory("Math")]
[NodeTags("Exponent", "Exp", "e^", "e", "Exponential")]
public class ExponentMathNode : INode
{
    public string Name => "Exponent";

    public void Execute([NodeInput("Value")] double value, [NodeOutput("Result")] out double result)
    {
        result = System.Math.Exp(value);
    }
}