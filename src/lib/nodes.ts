import { get, writable } from "svelte/store";
import { sendMessage } from "./ws";

export let structures = writable<NodeStructure[]>([]);
export let types = writable<TypeDefinition[]>([]);

export let instances = writable<NodeInstance[]>([]);
export let connections = writable<NodeInstanceConnection[]>([]);

export let result = writable<WebResult>({ Instances: [], Values: [] } as WebResult);

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
    NodeType: string;
    Properties: Property[];
    Inputs: Parameter[];
    Outputs: Parameter[];
    ExecuteMethod: string;
}

export interface Parameter {
    Id: string;
    Name: string;
    Type: string;
}

export interface Property {
    Name: string;
    Type: string;
    HtmlType: string;
    DefaultValue: string;
}

export interface TypeDefinition {
    Name: string;
    Type: string;
    Color: string;
    Shape: string;
}

export interface NodeInstance {
    Id: string;
    NodeType: string;
    Properties: PropertyInstance[];
    Metadata: NodeMetadata;
}

export interface NodeMetadata {
    Position: NodePosition;
}

export interface NodePosition {
    X: number;
    Y: number;
    x: number;
    y: number;
}
export interface PropertyInstance {
    Name: string;
    Type: string;
    Value: string;
    DefaultValue: any;
}
export interface NodeInstanceConnection {
    From: ConnectedProperty;
    To: ConnectedProperty;
}

export interface ConnectedProperty {
    InstanceId: string;
    PropertyId: string;
}

export interface WebResult {
    Values: ParameterValue[];
    Instances: InstanceValue[];
}

export interface ParameterValue {
    Id: string;
    Value: string;
}

export interface InstanceValue {
    Id: string;
    Value: {
        Name: string;
    };
}
