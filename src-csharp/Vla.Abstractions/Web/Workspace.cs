using System.Collections.Immutable;
using System.Drawing;
using Newtonsoft.Json;
using Vla.Abstractions.Types;

namespace Vla.Abstractions.Web;

public readonly struct Workspace(string name)
{
	/// <summary>
	/// The name of the workspace.
	/// </summary>
	[JsonProperty("name")]
	public string Name { get; init; } = name;

	/// <summary>
	/// The accent color of the workspace.
	/// </summary>
	[JsonProperty("color")]
	public NodeTypeDefinition.ColorDefinition Color { get; init; } = System.Drawing.Color.Wheat;
	
	/// <summary>
	/// The webs contained in the workspace.
	/// </summary>
	[JsonProperty("webs")]
	public ImmutableArray<Web> Webs { get; init; } = ImmutableArray<Web>.Empty;
}