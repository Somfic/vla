namespace Vla.Abstractions.Structure;

public interface IParameterStructure
{
	public string Id { get; init; }
	
	public string Name { get; init; }
	
	public string Description { get; init; }
	
	public Type Type { get; init; }
}