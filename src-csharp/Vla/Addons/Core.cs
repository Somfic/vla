using Microsoft.Extensions.DependencyInjection;

namespace Vla.Addons;

public class CoreAddon : Addon.Metadata.Addon
{
	public override string Name => "Core";
	
	public override string Abstract => "Core addon for Vla";
	
	public override Guid Identifier => new("b8046b83-40b4-4aa8-b895-cf4eeae1862e");
	
	public override Uri DownloadUri => new("");
	
	public override string Author => "Vla";
	
	public override Version Version => typeof(CoreAddon).Assembly.GetName().Version ?? new Version(0, 0, 0);
	
	public override Version VlaVersion => new(0, 0, 0);
	
	public override IServiceCollection ConfigureServices(IServiceCollection services)
	{
		return services;
	}
}

