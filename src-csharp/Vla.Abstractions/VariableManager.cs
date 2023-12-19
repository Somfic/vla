using System.Collections.Immutable;
using Vla.Helpers;

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
        if (_variables.TryGetValue(variable, out var raw))
        {
            var defaultValue = typeof(T).GetDefaultValueForType();
            if (raw is null)
                return (defaultValue is T value ? value : default) ?? throw new Exception($"Could not find variable {variable}");
        }
        return (T)raw!;
    }
}