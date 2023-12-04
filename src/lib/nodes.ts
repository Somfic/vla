import { get, writable } from "svelte/store";
import { sendMessage } from "./ws";

export let structures = writable<NodeStructure[]>([]);
export let types = writable<TypeDefinition[]>([]);

export let instances = writable<NodeInstance[]>([]);
export let connections = writable<NodeInstanceConnection[]>([]);

export let result = writable<WebResult>({ instances: [], values: [] } as WebResult);

export function runWeb() {
    let message = {
        Web: {
            Instances: get(instances),
            Connections: get(connections),
        },
        Id: "RunWeb",
    };

    sendMessage(message);
}

export interface NodeStructure {
    nodeType: string;
    category: string;
    searchTerms: string[];
    properties: Property[];
    inputs: Parameter[];
    outputs: Parameter[];
    executeMethod: string;
}

export interface Parameter {
    id: string;
    name: string;
    type: string;
}

export interface Property {
    name: string;
    type: string;
    htmlType: string;
    defaultValue: string;
}

export interface TypeDefinition {
    name: string;
    type: string;
    values: TypeDefinitionValue[];
    color: string;
    shape: string;
}

export interface TypeDefinitionValue {
    name: string;
    value: string;
}

export interface NodeInstance {
    id: string;
    nodeType: string;
    properties: PropertyInstance[];
    metadata: NodeMetadata;
}

export interface NodeMetadata {
    position: NodePosition;
}

export interface NodePosition {
    x: number;
    y: number;
}
export interface PropertyInstance {
    name: string;
    type: string;
    value: string;
    defaultValue: any;
}
export interface NodeInstanceConnection {
    from: ConnectedProperty;
    to: ConnectedProperty;
}

export interface ConnectedProperty {
    instanceId: string;
    propertyId: string;
}

export interface WebResult {
    values: ParameterValue[];
    instances: InstanceValue[];
}

export interface ParameterValue {
    id: string;
    value: string;
}

export interface InstanceValue {
    id: string;
    value: {
        name: string;
    };
}
