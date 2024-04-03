using System.Collections.Immutable;
using Somfic.Common;
using Vla.Server.Messages;
using Vla.Server.Messages.Requests;
using Vla.Server.Messages.Response;
using Vla.Services;
using Vla.Workspace;

namespace Vla.Server.Methods;

[ServerMethods("workspace")]
public class WorkspaceMethods : IServerMethods
{
	private readonly WorkspaceService _workspaces;

	public WorkspaceMethods(WorkspaceService workspaces)
	{
		_workspaces = workspaces;
	}

	[ServerMethod("create")]
	public ISocketResponse Create(CreateWorkspaceRequest request)
	{
		var workspace = _workspaces.CreateOrLoad(request.Name, request.Path);

		return workspace
			.Map<ISocketResponse, Abstractions.Workspace>(
				w => new WorkspaceResponse(w),
				e => new ExceptionResponse(e));
	}
	
	[ServerMethod("save")]
	public void Save(WorkspaceRequest request) => _workspaces.Save(request.Workspace);
	
	[ServerMethod("list")]
	public ISocketResponse List()
	{ 
		var workspaces = _workspaces.List();

		if (workspaces.IsValue && workspaces.Expect().Length == 0)
		{
			Create(new CreateWorkspaceRequest { Name = "Untitled", Path = "Untitled.vla" });
		}

		return _workspaces.List()
			.Map<ISocketResponse, ImmutableArray<Abstractions.Workspace>>(
				w => new WorkspacesResponse(w),
				e => new ExceptionResponse(e));
	}

	[ServerMethod("delete")]
	public void Delete(WorkspaceRequest request) => _workspaces.Delete(request.Workspace);
}