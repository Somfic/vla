import type { Web, Workspace } from './nodes';
import { writable, get, type Writable } from 'svelte/store';

// State
export const workspaces = writable([] as Workspace[]);
export const workspaceName = writable('');
export const webId = writable('');

// Derived
export const workspace: Writable<Workspace | undefined> = writable();
export const web: Writable<Web | undefined> = writable();

export function reset() {
	workspaces.set([]);
	workspaceName.set('Workspace');
	webId.set('');

	workspaces.subscribe(() => setWorkspace());
	workspaces.subscribe(() => setWeb());

	workspaceName.subscribe(() => setWorkspace());
	workspaceName.subscribe(() => setWeb());

	webId.subscribe(() => setWorkspace());
	webId.subscribe(() => setWeb());
}

function setWorkspace() {
	workspace.set(get(workspaces).find((w) => w.name == get(workspaceName)));
}

function setWeb() {
	web.set(get(workspace)?.webs.find((w) => w.id == get(webId)));
}
