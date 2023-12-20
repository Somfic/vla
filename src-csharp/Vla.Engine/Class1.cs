using System.Collections.Immutable;

namespace Vla.Engine;

/// <summary>
///  A topological sorter for nodes.
/// </summary>
public readonly struct TopologicalSorter(ImmutableArray<(string from, string to)> connections)
{
	public ImmutableDictionary<string, ImmutableArray<string>> Graph { get; } = CreateGraph(connections);
	
	public ImmutableArray<string> FindDependencies(string nodeId)
	{
		var visited = new HashSet<string>();
		var stack = new Stack<string>();

		Visit(nodeId, ref visited, ref stack);

		var sortedNodes = new List<string>();

		while (stack.Count > 0)
		{
			var id = stack.Pop();
			sortedNodes.Add(id);
		}

		return sortedNodes
			.Skip(1) // Skip the first node, since it's the node we're looking for dependencies for.
			.ToImmutableArray();
	}
	
	public ImmutableArray<string> Sort()
	{
		var visited = new HashSet<string>();
		var stack = new Stack<string>();

		foreach (var nodeId in Graph.Keys)
		{
			Visit(nodeId, ref visited, ref stack);
		}

		var sortedNodes = new List<string>();

		while (stack.Count > 0)
		{
			var nodeId = stack.Pop();
			sortedNodes.Add(nodeId);
		}

		return sortedNodes.ToImmutableArray();
	}

	private void Visit(string nodeId, ref HashSet<string> visited, ref Stack<string> stack)
	{
		if(!Graph.TryGetValue(nodeId, out var value))
			throw new ArgumentException($"The node with id {nodeId} does not exist in the graph.");
		
		if (!visited.Add(nodeId))
			return;

		foreach (var childId in value)
		{
			Visit(childId, ref visited, ref stack);
		}
		
		stack.Push(nodeId);
	}

	private static ImmutableDictionary<string, ImmutableArray<string>> CreateGraph(ImmutableArray<(string from, string to)> connections)
	{
		var graph = new Dictionary<string, ImmutableArray<string>>();

		foreach (var connection in connections)
		{
			if(connection.from == connection.to)
				throw new ArgumentException("A node cannot be connected to itself.");
			
			if (!graph.ContainsKey(connection.from))
				graph.Add(connection.from, []);

			if (!graph.ContainsKey(connection.to))
				graph.Add(connection.to, []);

			graph[connection.to] = graph[connection.to]
				.Add(connection.from)
				.Distinct()
				.ToImmutableArray();
		}

		return graph.ToImmutableDictionary();
	}
}