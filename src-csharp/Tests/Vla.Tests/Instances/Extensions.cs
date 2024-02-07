using Newtonsoft.Json;
using Somfic.Common;
using Vla.Abstractions.Instance;
using Vla.Addon;
using Vla.Nodes;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Vla.Tests.Instances;

public class Extensions
{
	[Node(Purity.Deterministic)]
	[NodeCategory("Testing")]
	[NodeTags("Tag")]
	public class ValidNode : INode
	{
		public string Name => "Computed node name";
		
		[NodeProperty("Property name")]
		public int Property { get; set; }
		
		public void Execute([NodeOutput("Output name")] out int output, [NodeOutput] out int outputPlus1, [NodeInput("Input name")] int input = 1)
		{
			output = input;
			outputPlus1 = input + 1;
		}
	}
	
	[Test]
	public void From_ValidNode_HasUniqueId()
	{
		var structure = NodeExtensions.ToStructure<ValidNode>().Expect();
		var instance = new NodeInstance().From(structure);

		Assert.That(instance.Id, Is.Not.Empty);
		Assert.That(instance.Id, Is.Not.EqualTo(Guid.Empty));
	}

	[Test]
	public void From_ValidNode_HasCorrectType()
	{
		var structure = NodeExtensions.ToStructure<ValidNode>().Expect();
		var instance = new NodeInstance().From(structure);

		Assert.That(instance.NodeType, Is.EqualTo(typeof(ValidNode)));
	}

	[Test]
	public void From_ValidNode_HasCorrectProperties()
	{
		var structure = NodeExtensions.ToStructure<ValidNode>().Expect();
		var instance = new NodeInstance().From(structure);

		instance = instance.WithProperty(nameof(ValidNode.Property), 12);

		Assert.That(instance.Properties, Has.Length.EqualTo(1));
		Assert.That(instance.Properties[0].Id, Is.EqualTo(nameof(ValidNode.Property)));
		Assert.That(instance.Properties[0].Value, Is.EqualTo(JsonConvert.SerializeObject(12)));
		Assert.That(instance.Properties[0].Type, Is.EqualTo(typeof(int)));
	}
}