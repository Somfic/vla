using System.Collections.Immutable;
using Somfic.Common;
using Vla.Server.Messages;
using Vla.Server.Messages.Requests;
using Vla.Server.Messages.Response;
using Vla.Workspace;
using WatsonWebsocket;

namespace Vla.Server.Methods;

public class Workspaces : IServerMethods
{
	private readonly WorkspaceService _workspaces;

	public Workspaces(WorkspaceService workspaces)
	{
		_workspaces = workspaces;
	}
	
	[Request("workspaces-list-recent")]
	private async Task<ISocketResponse> OnWorkspacesListRecent(ClientMetadata client)
	{
		var workspaces = await _workspaces.ListRecentAsync();

		return workspaces
			.Map<ISocketResponse, ImmutableArray<Abstractions.Workspace>>(
				w => new WorkspacesResponse(w),
				e => new ExceptionResponse(e));
	}

	[Request("workspace-create")]
	private async Task<ISocketResponse> OnWorkspaceCreate(ClientMetadata client, CreateWorkspaceRequest request)
	{
		var workspace = await _workspaces.CreateOrLoadAsync(request.Name, request.Path);

		return workspace
			.Map<ISocketResponse, Abstractions.Workspace>(
				w => new WorkspaceResponse(w),
				e => new ExceptionResponse(e));
	}
	
	[Request("workspace-save")]
	private async Task OnWorkspaceSave(ClientMetadata client, WorkspaceRequest request)
	{
		await _workspaces.SaveAsync(request.Workspace);
	}
}

public interface IServerMethods
{
}