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
		
		Assert.That(instance.Properties, Has.Length.EqualTo(structure.Properties.Length));
	}
}