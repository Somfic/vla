namespace Vla.Nodes.Structure;

public interface IParameterStructure
{
	public string Id { get; init; }
	
	public string Name { get; init; }
	
	public Type Type { get; init; }
}