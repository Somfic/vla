using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace Vla.Addon.Metadata;

public abstract class Addon
{
	/// <summary>
	///     The display name of this addon.
	/// </summary>
	public abstract string Name { get; }

	/// <summary>
	///     A short description of this addon.
	/// </summary>
	public abstract string Abstract { get; }

	/// <summary>
	///     A unique identifier of this addon.
	/// </summary>
	public abstract Guid Identifier { get; }

	/// <summary>
	///     The download URI of this addon.
	/// </summary>
	public abstract Uri DownloadUri { get; }

	/// <summary>
	///     The author of this addon.
	/// </summary>
	public abstract string Author { get; }

	/// <summary>
	///     The version of this addon.
	/// </summary>
	public abstract Version Version { get; }

	/// <summary>
	///     The release status of this addon.
	/// </summary>
	public virtual ReleaseStatus ReleaseStatus { get; } = ReleaseStatus.Stable;

	/// <summary>
	///     The minimum version of Vla required to run this addon.
	/// </summary>
	public abstract Version VlaVersion { get; }

	/// <summary>
	///     A collection of resources related to this addon.
	/// </summary>
	public virtual Uris Uris { get; } = new();

	/// <summary>
	///     A collection of tags describing this addon.
	/// </summary>
	public virtual IReadOnlyCollection<string> Tags { get; } = Array.Empty<string>();

	/// <summary>
	///     A collection of dependencies required by this addon.
	/// </summary>
	public virtual IReadOnlyCollection<Dependency> Dependencies { get; } = Array.Empty<Dependency>();

	/// <summary>
	///     A collection of addons that are recommended to be used with this addon.
	/// </summary>
	public virtual IReadOnlyCollection<Recommendation> Recommendations { get; } = Array.Empty<Recommendation>();

	/// <summary>
	///     A collection of addons that conflict with this addon.
	/// </summary>
	public virtual IReadOnlyCollection<Conflict> Conflicts { get; } = Array.Empty<Conflict>();

	public abstract IServiceCollection ConfigureServices(IServiceCollection services);
}

public enum ReleaseStatus
{
	Alpha,
	Beta,
	ReleaseCandidate,
	Stable
}

public struct Uris()
{
	public Uri? LicenseUri { get; set; } = null;

	public Uri? ReadmeUri { get; set; } = null;

	public Uri? HomePageUri { get; set; } = null;

	public Uri? SourceCodeUri { get; set; } = null;

	public Uri? IssueTrackerUri { get; set; } = null;

	public Uri? DocumentationUri { get; set; } = null;
}

public readonly struct Dependency(string name, Version minVersion)
{
	public static implicit operator Dependency((string name, Version minVersion) tuple)
	{
		return new Dependency(tuple.name, tuple.minVersion);
	}

	[JsonProperty("name")]
	public string Name { get; } = name;

	[JsonProperty("minVersion")]
	public Version MinVersion { get; } = minVersion;
}

public readonly struct Recommendation(string name)
{
	public static implicit operator Recommendation(string name)
	{
		return new Recommendation(name);
	}

	public string Name { get; } = name;
}

public readonly struct Conflict(string name)
{
	public static implicit operator Conflict(string name)
	{
		return new Conflict(name);
	}

	public string Name { get; } = name;
}