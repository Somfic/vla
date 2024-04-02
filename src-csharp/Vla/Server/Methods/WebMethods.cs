using System.Collections.Immutable;
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
	private readonly ILogger<WebMethods> _log;
	private readonly WorkspaceService _workspace;

	public WebMethods(WorkspaceService workspace, ILogger<WebMethods> log)
	{
		_log = log;
		_workspace = workspace;
		
	}
	
	[ServerMethod("create")]
	public ISocketResponse Create(WebByNameRequest request)
	{
		_log.LogInformation("Creating web '{Name}' in '{WorkspacePath}'", request.Name, request.WorkspacePath);
		
		return _workspace.Load(request.WorkspacePath)
			.Pipe(x => _workspace.CreateWeb(x, request.Name))
			.Pipe(_ => _workspace.List())
			.Map<ISocketResponse, ImmutableArray<Abstractions.Workspace>>(
				w => new WorkspacesResponse(w),
				e => new ExceptionResponse(e));
	}

	[ServerMethod("update")]
	public ISocketResponse Update(WebRequest request)
	{
		_log.LogInformation("Updating web '{Name}' in '{WorkspacePath}'", request.Web.Name, request.Web.WorkspacePath);
		
		return _workspace.UpdateWeb(request.Web)
			.Pipe(_ => _workspace.List())
			.Map<ISocketResponse, ImmutableArray<Abstractions.Workspace>>(
				w => new WorkspacesResponse(w),
				e => new ExceptionResponse(e));
	}
}