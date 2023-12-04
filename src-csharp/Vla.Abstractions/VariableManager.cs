using System.Collections.Immutable;

namespace Vla.Abstractions;

public class VariableManager
{
	private ImmutableDictionary<string, object?> _variables = ImmutableDictionary<string, object?>.Empty;
	
	public void SetVariable<T>(string variable, T value)
	{
		_variables = _variables.SetItem(variable, value);
	}
	
	public T GetVariable<T>(string variable)
	{
		if (_variables.TryGetValue(variable, out var raw)) return (T)typeof(T).GetDefaultValueForType();
		return (T)raw!;
	}
}