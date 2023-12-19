﻿using Vla.Abstractions;
using Vla.Abstractions.Attributes;
using Vla.Nodes;

namespace Vla.Extensions.Core.Variables;

[Node("Get int variable")]
[NodeCategory("Variables")]
[NodeTags("Get", "Int", "Integer", "Load")]
public class GetIntVariable(VariableManager variableManager) : INode
{
    public string Name => "Get int variable";

    public void Execute([NodeInput("Variable")] string variable, [NodeOutput("Value")] out int value)
    {
        value = variableManager.GetVariable<int>(variable);
    }
}