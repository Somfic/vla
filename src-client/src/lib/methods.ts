import type { Web, Workspace } from './models/workspace';
import { sendMessage } from './ws';

export function createWorkspace(name: string, path: string) {
	sendMessage('workspace create', { name, path });
}

export function saveWorkspace(workspace: Workspace) {
	sendMessage('workspace save', { workspace });
}

export function listWorkspaces() {
	sendMessage('workspace list');
	// await waitForResponse('workspace list');
	// TODO: await a response from the server and return it in a promise
}

export function deleteWorkspace(workspace: Workspace) {
	sendMessage('workspace delete', { workspace });
}

export function createWeb(workspace: Workspace, name: string) {
	sendMessage('web create', { workspacePath: workspace.path, name });
}

export function updateWeb(workspace: Workspace, web: Web) {
	sendMessage('web update', { workspacePath: workspace.path, web });
}
