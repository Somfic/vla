namespace Vla.Addon.Services;

public interface IVariableManager
{
	void SetVariable<T>(string variable, T value);
	T GetVariable<T>(string variable);
}