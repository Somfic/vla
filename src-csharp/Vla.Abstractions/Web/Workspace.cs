using System.Collections.Immutable;
using Newtonsoft.Json;
using Vla.Addon.Metadata;
using Vla.Nodes;

namespace Vla.Abstractions.Web;

public readonly struct Workspace(string name)
{
    /// <summary>
    /// The name of the workspace.
    /// </summary>
    [JsonProperty("name")]
    public string Name { get; init; } = name;

    /// <summary>
    /// The path this workspaces has been loaded from. Automatically set when loading.
    /// </summary>
    [JsonProperty("path")]
    public string Path { get; init; } = string.Empty;

    /// <summary>
    /// The date and time this workspace was created.
    /// </summary>
    [JsonProperty("created")]
    public DateTime Created { get; init; } = DateTime.Now;

    /// <summary>
    /// The date and time this workspace was last modified. Automatically set when saving.
    /// </summary>
    [JsonProperty("lastModified")]
    public DateTime LastModified { get; init; } = DateTime.Now;

    /// <summary>
    /// The webs contained in the workspace.
    /// </summary>
    [JsonProperty("webs")]
    public ImmutableArray<Web> Webs { get; init; } = ImmutableArray<Web>.Empty;

    /// <summary>
    /// The structures possibly contained in the workspace. This is automatically set when loading.
    /// </summary>
    [JsonProperty("structures")]
    public ImmutableArray<NodeStructure> Structures { get; init; } = ImmutableArray<NodeStructure>.Empty;

    /// <summary>
    /// The types possibly contained in the workspace. This is automatically set when loading.
    /// </summary>
    [JsonProperty("types")]
    public ImmutableArray<TypeDefinition> Types { get; init; } = ImmutableArray<TypeDefinition>.Empty;

    /// <summary>
    /// The extensions active in the workspace.
    /// </summary>
    [JsonProperty("addons")]
    public ImmutableArray<Dependency> Addons { get; init; } = ImmutableArray<Dependency>.Empty;
}