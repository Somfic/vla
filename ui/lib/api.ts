import { createTauRPCProxy, type NodeData } from './core'
import type { Node } from '@xyflow/svelte';

export type VlaNode = Node<NodeData>;

let saveCallback: (() => void) | null = null;

export const setSaveCallback = (callback: () => void) => {
    saveCallback = callback;
};

export const saveNodeChanges = () => {
    saveCallback?.();
};

export default createTauRPCProxy();
