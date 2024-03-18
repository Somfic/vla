import type { Workspace } from './models/workspace';
import { sendMessage } from './ws';

export function createWorkspace(name: string, path: string) {
	sendMessage({ id: 'workspace create', data: { name, path } });
}

export function saveWorkspace(workspace: Workspace) {
	sendMessage({ id: 'workspace save', data: { workspace } });
}

export function listWorkspaces() {
	sendMessage({ id: 'workspace list', data: null });
	await waitForResponse('workspace list');
	// TODO: await a response from the server and return it in a promise
}

export function deleteWorkspace(workspace: Workspace) {
	sendMessage({ id: 'workspace delete', data: { workspace } });
}

export function createWeb(workspace: Workspace, name: string) {
	sendMessage({ id: 'web create', data: { workspacePath: workspace.path, name } });
}
