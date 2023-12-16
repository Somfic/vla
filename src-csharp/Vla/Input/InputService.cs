using Microsoft.Extensions.Logging;
using SharpHook.Native;

namespace Vla.Input;

public class InputService
{
	private readonly ILogger<InputService> _log;

	public InputService(ILogger<InputService> log)
	{
		_log = log;
	}
	
	public void Press(string key)
	{
		UioHook.PostText(key);
	}
}