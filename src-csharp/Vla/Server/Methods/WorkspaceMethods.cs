using System.Collections.Immutable;
using Somfic.Common;
using Vla.Server.Messages;
using Vla.Server.Messages.Requests;
using Vla.Server.Messages.Response;
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
	public async Task<ISocketResponse> Create(CreateWorkspaceRequest request)
	{
		var workspace = await _workspaces.CreateOrLoadAsync(request.Name, request.Path);

		return workspace
			.Map<ISocketResponse, Abstractions.Workspace>(
				w => new WorkspaceResponse(w),
				e => new ExceptionResponse(e));
	}
	
	[ServerMethod("save")]
	public Task Save(WorkspaceRequest request) => _workspaces.SaveAsync(request.Workspace);
	
	[ServerMethod("list")]
	public async Task<ISocketResponse> List()
	{
		var workspaces = await _workspaces.ListAsync();

		return workspaces
			.Map<ISocketResponse, ImmutableArray<Abstractions.Workspace>>(
				w => new WorkspacesResponse(w),
				e => new ExceptionResponse(e));
	}

	[ServerMethod("delete")]
	public void Delete(WorkspaceRequest request) => _workspaces.Delete(request.Workspace);
}