﻿using Vla.Abstractions;
using Vla.Abstractions.Attributes;
using Vla.Nodes;

namespace Vla.Extensions.Core.Variables;

[Node("Get boolean variable")]
[NodeCategory("Variables")]
[NodeTags("Get", "Boolean", "Bool", "Load")]
public class GetBooleanVariable(VariableManager variableManager) : INode
{
    public string Name => "Get boolean variable";

    public void Execute([NodeInput("Variable")] string variable, [NodeOutput("Value")] out bool value)
    {
        value = variableManager.GetVariable<bool>(variable);
    }
}