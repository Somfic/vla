using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Vla.Nodes.Connection;
using Vla.Nodes.Instance;

namespace Vla.Tests.Engine;

public class TopologicalSorter
{
	private const int RandomIterations = 100;
	
	private readonly (string from, string to)[] _nominalData =
	[
		("1", "2"),
		("1", "3"),
		("2", "4"),
		("2", "5"),
		("3", "4"),
		("4", "5")
	];
	
	private readonly (string from, string to)[] _loopingData =
	[
		("A", "B"),
		("B", "C"),
		("C", "D"),
		("D", "E"),
		("E", "A")
	];

	private readonly (string from, string to)[] _invalidLoopingData = [("A", "A")];
	
	[Test]
	public void TopologicalSorter_Constructor_ThrowsOnLoopingNode()
	{
		Assert.Throws<ArgumentException>(() => _ = new Vla.Engine.TopologicalSorter(_invalidLoopingData));
	}
	
	[Test]
	public void TopologicalSorter_FindDependencies_WorksWithNominal()
	{
		var random = new Random();
		
		for (var i = 0; i < RandomIterations; i++)
		{
			var randomOrder = _nominalData.OrderBy(_ => random.Next()).ToArray();
			
			var sorter = new Vla.Engine.TopologicalSorter(randomOrder);
			
			Assert.That(sorter.FindDependencies("1"), Is.Empty);
			Assert.That(sorter.FindDependencies("2").Sort(), Is.EquivalentTo(new[] { "1" }));
			Assert.That(sorter.FindDependencies("3").Sort(), Is.EquivalentTo(new[] { "1" }));
			Assert.That(sorter.FindDependencies("4").Sort(), Is.EquivalentTo(new[] { "1", "2", "3" }));
			Assert.That(sorter.FindDependencies("5").Sort(), Is.EquivalentTo(new[] { "1", "2", "3", "4" }));
		}
	}
	
	[Test]
	public void TopologicalSorter_FindDependencies_DoesNotGetStuckInLoop()
	{
		var random = new Random();
		
		for (var i = 0; i < RandomIterations; i++)
		{
			var randomOrder = _loopingData.OrderBy(_ => random.Next()).ToArray();
			
			var sorter = new Vla.Engine.TopologicalSorter(randomOrder);
			
			Assert.That(sorter.FindDependencies("A").Sort(), Is.EquivalentTo(new[] { "B", "C", "D", "E" }));
			Assert.That(sorter.FindDependencies("B").Sort(), Is.EquivalentTo(new[] { "A", "C", "D", "E" }));
			Assert.That(sorter.FindDependencies("C").Sort(), Is.EquivalentTo(new[] { "A", "B", "D", "E" }));
			Assert.That(sorter.FindDependencies("D").Sort(), Is.EquivalentTo(new[] { "A", "B", "C", "E" }));
			Assert.That(sorter.FindDependencies("E").Sort(), Is.EquivalentTo(new[] { "A", "B", "C", "D" }));
		}
	}
	
	[Test]
	public void TopologicalSorter_FindDependencies_ThrowsOnNonExistentNode()
	{
		var sorter = new Vla.Engine.TopologicalSorter(_nominalData);
			
		Assert.Throws<ArgumentException>(() => sorter.FindDependencies("X"));
	}
	
	[Test]
	public void TopologicalSorter_Sort_WorksWithNominal()
	{
		var random = new Random();
		
		for (var i = 0; i < RandomIterations; i++)
		{
			var randomOrder = _nominalData.OrderBy(_ => random.Next()).ToArray();
			
			var sorter = new Vla.Engine.TopologicalSorter(randomOrder);
			
			Assert.That(sorter.Sort().Select(x => x.value), Is.EquivalentTo(new[] { "1", "2", "3", "4", "5" }));
			Assert.That(sorter.Sort().Select(x => x.dependencies), Is.EquivalentTo(new[] { 2, 2, 1, 1, 0 }));
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
			
			Assert.That(sorter.Sort().Select(x => x.value), Is.EquivalentTo(new[] { "A", "E", "D", "C", "B" }));
			Assert.That(sorter.Sort().Select(x => x.dependencies), Is.EquivalentTo(new[] { 1, 1, 1, 1, 1 }));
		}
	}
}