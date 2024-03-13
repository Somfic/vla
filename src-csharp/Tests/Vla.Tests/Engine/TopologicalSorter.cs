namespace Vla.Tests.Engine;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

public class TopologicalSorter
{
	private const int RandomIterations = 100;

	private readonly (string from, string to)[] _loopingData =
	[
		("A", "B"),
		("B", "C"),
		("C", "D"),
		("D", "E"),
		("E", "A")
	];

	private readonly (string from, string to)[] _multipleDependenciesData =
	[
		("Power", "Add"),
		("Add", "Square root")
	];

	private readonly (string from, string to)[] _nominalData =
	[
		("1", "2"),
		("1", "3"),
		("2", "4"),
		("2", "5"),
		("3", "4"),
		("4", "5")
	];

	[Test]
	public void TopologicalSorter_Sort_WorksWithNominal()
	{
		var random = new Random();

		for (var i = 0; i < RandomIterations; i++)
		{
			var randomOrder = _nominalData.OrderBy(_ => random.Next()).ToArray();

			var sorter = new Vla.Engine.TopologicalSorter(randomOrder);

			Assert.That(sorter.Sort().Select(x => x), Is.EquivalentTo(new[] { "1", "2", "3", "4", "5" }));
		}
	}

	[Test]
	public void TopologicalSorter_Sort_DoesNotGetStuckInLoop()
	{
		var random = new Random();

		for (var i = 0; i < RandomIterations; i++)
		{
			var randomOrder = _loopingData.OrderBy(_ => random.Next()).ToArray();

			var sorter = new Vla.Engine.TopologicalSorter(randomOrder);

			Assert.That(sorter.Sort().Select(x => x), Is.EquivalentTo(new[] { "A", "E", "D", "C", "B" }));
		}
	}

	[Test]
	public void TopologicalSorter_Sort_WorksWithPythagorasTheorem()
	{
		var random = new Random();

		for (var i = 0; i < RandomIterations; i++)
		{
			var randomOrder = _multipleDependenciesData.OrderBy(_ => random.Next()).ToArray();

			var sorter = new Vla.Engine.TopologicalSorter(randomOrder);

			var sorted = sorter.Sort().ToArray();

			Assert.That(sorted, Has.Length.EqualTo(3));
			Assert.That(string.Join("->", sorted.Select(x => x)), Is.EqualTo("Power->Add->Square root"));
		}
	}
}