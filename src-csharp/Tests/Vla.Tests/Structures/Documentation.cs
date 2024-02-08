using Somfic.Common;
using Vla.Addon;
using Vla.Nodes;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Vla.Tests.Structures;

public class Documentation
{
	/// <summary>
	/// Node description
	/// </summary>
	[Node]
	[NodeCategory("Testing")]
	[NodeTags("Tag")]
	public class ValidNode : INode
	{
		public string Name => "Computed node name";
		
		/// <summary>
		///  Property description
		/// </summary>
		[NodeProperty("Property name")]
		public int Property { get; set; }
		
		/// <summary>
		/// Execute description
		/// </summary>
		/// <param name="output">Output description</param>
		/// <param name="outputPlus1">OutputPlus1 description</param>
		/// <param name="input"> Input description </param>
		public void Execute([NodeOutput("Output name")] out int output, [NodeOutput] out int outputPlus1, [NodeInput("Input name")] int input = 1)
		{
			output = input;
			outputPlus1 = input + 1;
		}
	}
	
	[Test]
	public void ToStructure_ValidNode_HasCorrectNodeDescription()
	{
		var structureResult = NodeExtensions.ToStructure<ValidNode>();

		if (structureResult.IsError)
			throw structureResult.Error.Expect();
		
		var structure = structureResult.Expect();
		
		Assert.That(structure.Description, Is.EqualTo("Node description"));
	}
	
	[Test]
	public void ToStructure_ValidNode_HasCorrectPropertyDescription()
	{
		var structureResult = NodeExtensions.ToStructure<ValidNode>();

		if (structureResult.IsError)
			throw structureResult.Error.Expect();
		
		var structure = structureResult.Expect();
		
		Assert.That(structure.Properties[0].Description, Is.EqualTo("Property description"));
	}
	
	[Test]
	public void ToStructure_ValidNode_HasCorrectExecuteInputDescription()
	{
		var structureResult = NodeExtensions.ToStructure<ValidNode>();

		if (structureResult.IsError)
			throw structureResult.Error.Expect();
		
		var structure = structureResult.Expect();
		
		Assert.That(structure.Inputs[0].Description, Is.EqualTo("Input description"));
	}
	
	[Test]
	public void ToStructure_ValidNode_HasCorrectExecuteOutputDescription()
	{
		var structureResult = NodeExtensions.ToStructure<ValidNode>();

		if (structureResult.IsError)
			throw structureResult.Error.Expect();
		
		var structure = structureResult.Expect();
		
		Assert.That(structure.Outputs[0].Description, Is.EqualTo("Output description"));
		Assert.That(structure.Outputs[1].Description, Is.EqualTo("OutputPlus1 description"));
	}
}