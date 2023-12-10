import { get, writable } from "svelte/store";
import { sendMessage } from "./ws";

export let structures = writable<NodeStructure[]>([]);
export let types = writable<TypeDefinition[]>([]);

export let result = writable<WebResult>({ instances: [], values: [] } as WebResult);
export let workspaces = writable<Workspace[]>([]);

export let workspaceId = writable<string>("");
export let webId = writable<string>("");

export function runWeb(web: Web) {
    setTimeout(() => {
        let message = {
            Web: web,
            Id: "RunWeb",
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

export interface Workspace {
    name: string;
    color: ColorDefinition;
    webs: Web[];
}

export interface Web {
    id: string;
    name: string;
    instances: NodeInstance[];
    connections: NodeInstanceConnection[];
}

export interface NodeStructure {
    nodeType: string;
    name: string;
    category: string;
    searchTerms: string[];
    properties: PropertyStructure[];
    inputs: ParameterStructure[];
    outputs: ParameterStructure[];
    executeMethod: string;
}

export interface ParameterStructure {
    id: string;
    name: string;
    type: string;
    defaultValue: any;
}

export interface PropertyStructure {
    name: string;
    type: string;
    htmlType: string;
}

export interface TypeDefinition {
    name: string;
    type: string;
    htmlType: string;
    values: TypeDefinitionValue[];
    defaultValue: any;
    color: ColorDefinition;
    shape: string;
}

export interface ColorDefinition {
    r: number;
    g: number;
    b: number;
    h: number;
    s: number;
    l: number;
    hex: string;
}

export interface TypeDefinitionValue {
    name: string;
    value: string;
}

export interface NodeInstance {
    id: string;
    nodeType: string;
    properties: PropertyInstance[];
    inputs: ParameterInstance[];
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
    values: ParameterInstance[];
    instances: InstanceValue[];
}

export interface ParameterInstance {
    id: string;
    value: string;
}

export interface InstanceValue {
    id: string;
    value: {
        name: string;
    };
}

export function typeToDefinition(type: string): TypeDefinition {
    return get(types).find((t) => t.type == type) ?? ({} as TypeDefinition);
}
