import type { Node, Edge, NodeProps } from "@xyflow/svelte";
import type { Brick } from "$lib/schema";

export type Point = { x: number; y: number };

/**
 * Per-node state held on the canvas. `brick` is a deep clone of the catalog
 * entry so two instances of the same brick don't share (and mutate) one
 * object. `arguments`/`defaults` are JSON-encoded string values keyed by the
 * brick's argument/input ids (same convention as the `main` branch).
 */
export type NodeData = {
	brickId: string;
	brick: Brick | null;
	arguments: Record<string, string>;
	defaults: Record<string, string>;
};

export type CanvasNode = Node<NodeData>;
export type CanvasNodeProps = NodeProps<CanvasNode>;

/**
 * In-memory graph. The backend currently only exposes the brick catalog
 * (`api.bricks.getBricks()`), so the graph lives entirely client-side until
 * save/load/execute commands exist on the rewrite backend. `<SvelteFlow>`
 * binds directly to `nodes`/`edges` and mutates them in place (drag, connect,
 * delete).
 */
class GraphStore {
	nodes = $state<CanvasNode[]>([]);
	edges = $state<Edge[]>([]);
	#seq = 0;

	addNode(brick: Brick, position: Point = { x: 0, y: 0 }): void {
		const id = `${brick.id}-${this.#seq++}`;

		const args: Record<string, string> = {};
		for (const a of brick.arguments) {
			if (a.defaultValue != null) args[a.id] = a.defaultValue;
		}

		const defaults: Record<string, string> = {};
		for (const i of brick.inputs) {
			if (i.defaultValue != null) defaults[i.id] = i.defaultValue;
		}

		this.nodes = [
			...this.nodes,
			{
				id,
				type: "v1",
				position,
				data: {
					brickId: brick.id,
					brick: structuredClone(brick),
					arguments: args,
					defaults,
				},
			},
		];
	}
}

export const graph = new GraphStore();
