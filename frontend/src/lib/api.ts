import { createTauRPCProxy, type NodeData } from './core'
import type { Node, NodeProps } from '@xyflow/svelte';

export type CanvasNode = Node<NodeData>;
export type CanvasNodeProps = NodeProps<CanvasNode>;

let saveCallback: (() => void) | null = null;

export const setSaveCallback = (callback: () => void) => {
    saveCallback = callback;
};

export const saveNodeChanges = () => {
    saveCallback?.();
};

export default createTauRPCProxy();
