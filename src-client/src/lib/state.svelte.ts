import { writable, get, type Writable } from 'svelte/store';
import type { Workspace } from './models/workspace';
import type { Web } from './models/web';
import type { NodeExecutionResult } from './models/result';

// State
export const workspaces = writable([] as Workspace[]);
export const workspaceName = writable('');
export const webName = writable('');
export const result = writable([] as NodeExecutionResult[]);

// Derived
export const workspace: Writable<Workspace | undefined> = writable();
export const web: Writable<Web | undefined> = writable();

export function reset() {
	workspaces.set([]);
	workspaceName.set('Untitled');
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
