using System.Collections.Immutable;

namespace Vla.Engine;

/// <summary>
///     A topological sorter for nodes.
/// </summary>
public readonly struct TopologicalSorter(params (string from, string to)[] connections)
{
	public ImmutableDictionary<string, ImmutableArray<string>> Graph { get; } =
		CreateGraph(connections.ToImmutableArray());

	private static ImmutableDictionary<string, ImmutableArray<string>> CreateGraph(
		ImmutableArray<(string from, string to)> connections)
	{
		var graph = new Dictionary<string, ImmutableArray<string>>();

		foreach (var connection in connections)
		{
			if (!graph.ContainsKey(connection.from))
				graph.Add(connection.from, ImmutableArray<string>.Empty);

			if (!graph.ContainsKey(connection.to))
				graph.Add(connection.to, ImmutableArray<string>.Empty);

			graph[connection.from] = graph[connection.from]
				.Add(connection.to)
				.Distinct()
				.ToImmutableArray();
		}

		return graph.ToImmutableDictionary();
	}

	public ImmutableArray<string> Sort()
	{
		var visited = new HashSet<string>();
		var stack = new Stack<string>();

		foreach (var nodeId in Graph.Keys)
			if (!visited.Contains(nodeId))
				Visit(nodeId, ref visited, ref stack);

		return stack.ToImmutableArray();
	}

	private void Visit(string nodeId, ref HashSet<string> visited, ref Stack<string> stack)
	{
		visited.Add(nodeId);

		foreach (var childId in Graph[nodeId])
			if (!visited.Contains(childId))
				Visit(childId, ref visited, ref stack);

		stack.Push(nodeId);
	}
}