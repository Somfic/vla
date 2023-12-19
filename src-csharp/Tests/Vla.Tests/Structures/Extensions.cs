using Newtonsoft.Json;
using Somfic.Common;
using Vla.Abstractions.Attributes;
using Vla.Nodes;

namespace Vla.Tests.Structures;

public class Extensions
{
	[Node("Initial node name")]
	[NodeCategory("Testing")]
	[NodeTags("Tag")]
	public class ValidNode : INode
	{
		public string Name => "Computed node name";
		
		[NodeProperty]
		public int Property { get; set; }
		
		public void Execute([NodeInput("Input name")] int input, [NodeOutput("Output name")] out int output, [NodeOutput] out int outputPlus1)
		{
			output = input;
			outputPlus1 = input + 1;
		}
	}
	
	[Test]
	public void ToStructure_ValidNode_HasCorrectType()
	{
		var structureResult = NodeExtensions.ToStructure<ValidNode>();

		if (structureResult.IsError)
			throw structureResult.Error.Expect();
		
		var structure = structureResult.Expect();
		
		Assert.That(structure.NodeType, Is.EqualTo(typeof(ValidNode)));
	}
	
	[Test]
	public void ToStructure_ValidNode_HasCorrectName()
	{
		var structureResult = NodeExtensions.ToStructure<ValidNode>();

		if (structureResult.IsError)
			throw structureResult.Error.Expect();
		
		var structure = structureResult.Expect();
		
		Assert.That(structure.Name, Is.EqualTo("Initial node name"));
	}

	[Test]
	public void ToStructure_ValidNode_HasCorrectCategory()
	{
		var structureResult = NodeExtensions.ToStructure<ValidNode>();

		if (structureResult.IsError)
			throw structureResult.Error.Expect();
		
		var structure = structureResult.Expect();
		
		Assert.That(structure.Category, Is.EqualTo("Testing"));
	}
	
	[Test]
	public void ToStructure_ValidNode_HasSearchTerms()
	{
		var structureResult = NodeExtensions.ToStructure<ValidNode>();
		
		if (structureResult.IsError)
			throw structureResult.Error.Expect();
		
		var structure = structureResult.Expect();
		
		Assert.That(structure.SearchTerms, Is.Not.Empty);
		Assert.That(structure.SearchTerms, Contains.Item(structure.Name));
		Assert.That(structure.SearchTerms, Contains.Item(structure.Category));
		Assert.That(structure.SearchTerms, Contains.Item("Tag"));
	}

	[Test]
	public void ToStructure_ValidNode_HasCorrectProperties()
	{
		var structureResult = NodeExtensions.ToStructure<ValidNode>();

		if (structureResult.IsError)
			throw structureResult.Error.Expect();
		
		var structure = structureResult.Expect();
		
		Assert.That(structure.Properties, Is.Not.Empty);
		Assert.That(structure.Properties, Has.Length.EqualTo(1));
		Assert.That(structure.Properties[0].Name, Is.EqualTo("Property"));
		Assert.That(structure.Properties[0].Type, Is.EqualTo(typeof(int)));
		Assert.That(structure.Properties[0].DefaultValue, Is.EqualTo(JsonConvert.SerializeObject(0)));
	}

	[Test]
	public void ToStructure_ValidNode_HasCorrectInputs()
	{
		var structureResult = NodeExtensions.ToStructure<ValidNode>();

		if (structureResult.IsError)
			throw structureResult.Error.Expect();
		
		var structure = structureResult.Expect();
		
		Assert.That(structure.Inputs, Is.Not.Empty);
		Assert.That(structure.Inputs.Length, Is.EqualTo(1));
		Assert.That(structure.Inputs[0].Id, Is.EqualTo("input"));
		Assert.That(structure.Inputs[0].Name, Is.EqualTo("Input name"));
		Assert.That(structure.Inputs[0].Type, Is.EqualTo(typeof(int)));
		Assert.That(structure.Inputs[0].DefaultValue, Is.EqualTo(JsonConvert.SerializeObject(0)));
	}
	
	[Test]
	public void ToStructure_ValidNode_HasCorrectOutputs()
	{
		var structureResult = NodeExtensions.ToStructure<ValidNode>();

		if (structureResult.IsError)
			throw structureResult.Error.Expect();
		
		var structure = structureResult.Expect();
		
		Assert.That(structure.Outputs, Is.Not.Empty);
		Assert.That(structure.Outputs.Length, Is.EqualTo(2));
		Assert.That(structure.Outputs[0].Id, Is.EqualTo("output"));
		Assert.That(structure.Outputs[0].Name, Is.EqualTo("Output name"));
		// Assert.That(structure.Outputs[0].Type, Is.EqualTo(typeof(int))); // FIXME: Find out a way to compare to Int32&, since this is an out parameter
		Assert.That(structure.Outputs[1].Id, Is.EqualTo("outputPlus1"));
		Assert.That(structure.Outputs[1].Name, Is.EqualTo("outputPlus1"));
		// Assert.That(structure.Outputs[1].Type, Is.EqualTo(typeof(int))); // FIXME: Find out a way to compare to Int32&, since this is an out parameter
	}
	
	[Test]
	public void ToStructure_ValidNode_HasCorrectExecuteMethod()
	{
		var structureResult = NodeExtensions.ToStructure<ValidNode>();

		if (structureResult.IsError)
			throw structureResult.Error.Expect();
		
		var structure = structureResult.Expect();
		
		Assert.That(structure.ExecuteMethod, Is.EqualTo("Execute"));
	}
}