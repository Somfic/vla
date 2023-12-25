import type { NodeInstance } from './models/instance';
import type { NodeStructure } from './models/structure';
import type { Web } from './models/web';
import type { Workspace } from './models/workspace';
import { workspaces } from './state.svelte';
import { sendMessage } from './ws';
import { get } from 'svelte/store';

export function runWeb(web: Web) {
	setTimeout(() => {
		let message = {
			Web: web,
			Id: 'RunWeb'
		};

		sendMessage(message);
	}, 1);
}

export function instanceFromId(id: string): NodeInstance {
	return (
		get(workspaces)
			.map((w) => w.webs)
			.flat()
			.map((w) => w.instances)
			.flat()
			.find((i) => i.id == id) ?? ({} as NodeInstance)
	);
}

export function saveWorkspace(workspace: Workspace) {
	let message = {
		workspace: workspace,
		id: 'save-workspace'
	};

	sendMessage(message);
}
