using System.Runtime.InteropServices;

public static class Plugin
{
	static void Log(string text)
	{
		using var block = Extism.Pdk.Allocate(text);
		wasm_Log(block.Offset);
	}

	[DllImport("extism", EntryPoint = "log")]
	static extern ulong wasm_Log(ulong offset);

	[UnmanagedCallersOnly(EntryPoint = "on_start")]
	static int OnStart()
	{
		Log("Hello from C#");
		return 0;
	}
}