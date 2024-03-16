using Microsoft.Extensions.Logging;
using Somfic.Common;
using Vla.Server.Messages;
using Vla.Server.Messages.Requests;
using Vla.Server.Messages.Response;
using Vla.Workspace;

namespace Vla.Server.Methods;

[ServerMethods("web")]
public class WebMethods : IServerMethods
{
	private readonly WorkspaceService _workspace;

	public WebMethods(WorkspaceService workspace)
	{
		_workspace = workspace;
	}
	
	[ServerMethod("create")]
	public async Task<ISocketResponse> Create(WebByNameRequest request)
	{
		return (await _workspace.LoadAsync(request.WorkspacePath))
			.Pipe(x => _workspace.CreateWebAsync(x, request.Name).GetAwaiter().GetResult()) // FIXME: There must be a better way to do async/await in a pipe... this is blocking...
			.Map<ISocketResponse, Abstractions.Web>(
				w => new WebResponse(w),
				e => new ExceptionResponse(e));
	}
}