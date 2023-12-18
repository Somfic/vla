import type { Workspace } from './nodes';

class State {
	workspaces = $state([] as Workspace[]);
	workspaceName = $state('');
	webId = $state('');

	workspace = $derived(this.workspaces.find((w) => w.name == this.workspaceName));
	web = $derived(findWeb(this));

	reset() {
		this.workspaces = [];
		this.workspaceName = 'Workspace';
		this.webId = '';
	}
}

export const state = new State();

function findWeb(state: State) {
	console.log('findWeb', state.webId);
	return state.workspace?.webs.find((w) => w.id == state.webId);
}
