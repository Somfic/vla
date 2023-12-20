using Newtonsoft.Json;
using Somfic.Common;
using Vla.Nodes;
using Vla.Nodes.Instance;

namespace Vla.Tests.Instances;

public class Extensions
{
	[Test]
	public void From_ValidNode_HasUniqueId()
	{
		var structure = NodeExtensions.ToStructure<Structures.Extensions.ValidNode>().Expect();
		var instance = new NodeInstance().From(structure);

		Assert.That(instance.Id, Is.Not.Null);
		Assert.That(instance.Id, Is.Not.Empty);
		Assert.That(instance.Id, Is.Not.EqualTo(Guid.Empty.ToString()));
		Assert.That(instance.Id, Has.Length.EqualTo(Guid.NewGuid().ToString().Length));
	}
	
	[Test]
	public void From_ValidNode_HasCorrectType()
	{
		var structure = NodeExtensions.ToStructure<Structures.Extensions.ValidNode>().Expect();
		var instance = new NodeInstance().From(structure);

		Assert.That(instance.NodeType, Is.EqualTo(typeof(Structures.Extensions.ValidNode)));
	}

	[Test]
	public void From_ValidNode_HasCorrectProperties()
	{
		var structure = NodeExtensions.ToStructure<Structures.Extensions.ValidNode>().Expect();
		var instance = new NodeInstance().From(structure);
		
		instance = instance.WithProperty(nameof(Structures.Extensions.ValidNode.Property), 12);
		
		Assert.That(instance.Properties, Has.Length.EqualTo(1));
		Assert.That(instance.Properties[0].Id, Is.EqualTo(nameof(Structures.Extensions.ValidNode.Property)));
		Assert.That(instance.Properties[0].Value, Is.EqualTo(JsonConvert.SerializeObject(12)));
		Assert.That(instance.Properties[0].Type, Is.EqualTo(typeof(int)));
	}
	
	[Test]
	public void From_ValidNode_HasCorrectInputs()
	{
		var structure = NodeExtensions.ToStructure<Structures.Extensions.ValidNode>().Expect();
		var instance = new NodeInstance().From(structure);
		
		Assert.That(instance.Inputs, Has.Length.EqualTo(1));
		Assert.That(instance.Inputs[0].Id, Is.EqualTo("input"));
	}
	
	[Test]
	public void From_ValidNode_HasCorrectOutputs()
	{
		var structure = NodeExtensions.ToStructure<Structures.Extensions.ValidNode>().Expect();
		var instance = new NodeInstance().From(structure);
		
		Assert.That(instance.Outputs, Has.Length.EqualTo(2));
		Assert.That(instance.Outputs[0].Id, Is.EqualTo("output"));
		Assert.That(instance.Outputs[1].Id, Is.EqualTo("outputPlus1"));
	}
}