import { writable, get, type Writable } from 'svelte/store';
import type { Web, Workspace } from './models/workspace';
import { invoke } from '@tauri-apps/api/tauri';

// State
export const workspaces = writable([] as Workspace[]);
export const workspaceName = writable('');
export const webName = writable('');

// Derived
export const workspace: Writable<Workspace | undefined> = writable();
export const web: Writable<Web | undefined> = writable();

let hasReceivedWorkspaces = false;
workspaces.subscribe((w) => {
	if (!hasReceivedWorkspaces && w.length > 0) {
		hasReceivedWorkspaces = true;
		workspaceName.set(get(workspaces)[0]?.name);
		invoke('close_splashscreen');
	}
});

export function reset() {
	workspaces.set([]);
	webName.set('');

	workspaces.subscribe(() => setWorkspace());
	workspaces.subscribe(() => setWeb());

	workspaceName.subscribe(() => setWorkspace());
	workspaceName.subscribe(() => setWeb());

	webName.subscribe(() => setWorkspace());
	webName.subscribe(() => setWeb());
}

function setWorkspace() {
	workspace.set(get(workspaces).find((w) => w.name == get(workspaceName)));
}

function setWeb() {
	web.set(get(workspace)?.webs.find((w) => w.name == get(webName)));
}
