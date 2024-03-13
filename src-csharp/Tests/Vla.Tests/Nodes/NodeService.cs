using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Vla.Addon;

namespace Vla.Tests.Nodes;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

public class NodeService
{
	[Test]
	public void NodeService_GeneratesCorrectTypes()
	{
		var nodes = CreateNodeService();

		var (_, types) = nodes.Generate();

		Assert.That(types.Select(x => x.Name), Contains.Item("NumberType"));

		var definition = types.First(x => x.Name == "NumberType");

		Assert.That(definition.PossibleValues.Select(x => x.Label), Is.EquivalentTo(new[] { "Integer", "Decimal" }));
		Assert.That(definition.PossibleValues.Select(x => x.Value),
			Is.EquivalentTo(new[] { NumberConstantNode.NumberType.Integer, NumberConstantNode.NumberType.Decimal }));
	}

	[Test]
	public void NodeService_Structure_CorrectName()
	{
		var nodes = CreateNodeService();

		var (structures, _) = nodes.Generate();

		Assert.That(structures.Select(x => x.Name), Contains.Item("NumberConstant"));
	}

	[Test]
	public void NodeService_Structure_CorrectCategory()
	{
		var nodes = CreateNodeService();

		var (structures, _) = nodes.Generate();

		var structure = structures.First(x => x.Name == "NumberConstant");

		Assert.That(structure.Category, Is.EqualTo("Number"));
	}

	[Test]
	[Ignore("Not implemented")]
	public void NodeService_Structure_CorrectDescription()
	{
	}

	[Test]
	public void NodeService_Structure_CorrectSearchTerms()
	{
		var nodes = CreateNodeService();

		var (structures, _) = nodes.Generate();

		var structure = structures.First(x => x.Name == "NumberConstant");

		Assert.That(structure.SearchTerms, Is.EquivalentTo(new[] { "Number", "Decimal", "Integer", "Custom" }));
	}

	[Test]
	public void NodeService_Structure_CorrectPurity()
	{
		var nodes = CreateNodeService();

		var (structures, _) = nodes.Generate();

		var structure = structures.First(x => x.Name == "NumberConstant");

		Assert.That(structure.Purity, Is.EqualTo(NodePurity.Deterministic));
	}

	[Test]
	public void NodeService_Structure_CorrectProperties()
	{
		var nodes = CreateNodeService();

		var (structures, _) = nodes.Generate();

		var structure = structures.First(x => x.Name == "NumberConstant");

		Assert.That(structure.Properties, Has.Length.EqualTo(2));
		Assert.That(structure.Properties.Select(x => x.Name), Is.EquivalentTo(new[] { "Value", "Mode" }));
		Assert.That(structure.Properties.Select(x => x.Type),
			Is.EquivalentTo(new[] { typeof(double), typeof(NumberConstantNode.NumberType) }));
		// Assert.That(structure.Properties.Select(x => x.Description), Is.EquivalentTo(new[] { null, null })); // TODO: Implement descriptions
	}


	private Vla.Nodes.NodeService CreateNodeService()
	{
		var services = Host.CreateDefaultBuilder()
			.ConfigureLogging(l =>
			{
				l.AddConsole();
				l.SetMinimumLevel(LogLevel.Trace);
			})
			.Build()
			.Services;

		return ActivatorUtilities.CreateInstance<Vla.Nodes.NodeService>(services);
	}

	[Node]
	[NodeCategory("Number")]
	[NodeTags("Custom")]
	public class NumberConstantNode : Node
	{
		public enum NumberType
		{
			[NodeEnumValue("Integer")]
			Integer,

			[NodeEnumValue("Decimal")]
			Decimal
		}

		public override string Name => "Number constant";

		[NodeProperty]
		public double Value { get; set; } = 100;

		[NodeProperty]
		public NumberType Mode { get; set; } = NumberType.Integer;

		public override Task Execute()
		{
			if (Value < 0)
				throw new ArgumentException("Value cannot be negative");

			switch (Mode)
			{
				case NumberType.Integer:
					Output("result", "Result", (int)Value);
					break;
				case NumberType.Decimal:
					Output("result", "Result", Value);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			return Task.CompletedTask;
		}
	}
}