using System.Collections.Concurrent;
using System.Collections.Immutable;
using Somfic.Common;

namespace Vla.Workspace;

public class LazyFile
{
	private static readonly ConcurrentDictionary<string, string> Files = new();
	
	public static void Write(string path, string content)
	{
		path = System.IO.Path.GetFullPath(path);
		Files.AddOrUpdate(path, content, (s, s1) => content);
	}

	public static string Read(string path)
	{
		return !Files.TryGetValue(path, out var file) ? string.Empty : file;
	}

	public static void Delete(string path)
	{
		Files.TryRemove(path, out _);
	}
	
	public static bool Exists(string path)
	{
		return Files.ContainsKey(path);
	}
}