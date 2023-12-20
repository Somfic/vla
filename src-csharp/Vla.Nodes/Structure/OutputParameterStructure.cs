using System.Reflection;
using Newtonsoft.Json;
using Vla.Helpers;
using Vla.Nodes.Attributes;

namespace Vla.Nodes.Structure;

public readonly struct OutputParameterStructure: IParameterStructure
{
	[JsonProperty("id")]
	public string Id { get; init; }

	[JsonProperty("name")]
	public string Name { get; init; } 
	
	[JsonProperty("description")]
	public string Description { get; init; } 

	[JsonProperty("type")]
	public Type Type { get; init; } 
	
	public static OutputParameterStructure FromParameterInfo(ParameterInfo parameterInfo)
	{
		return new OutputParameterStructure
		{
			Id = parameterInfo.Name!,
			Name = parameterInfo.GetCustomAttribute<NodeOutputAttribute>()?.Name ?? parameterInfo.Name!,
			Description = parameterInfo.GetDocumentation(),
			Type = parameterInfo.ParameterType
		};
	}
}