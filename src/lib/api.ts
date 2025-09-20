import { createTauRPCProxy, type NodeData } from './core'
import type { Node } from '@xyflow/svelte';

export type VlaNode = Node<NodeData>;

export default createTauRPCProxy();
