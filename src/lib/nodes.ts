import { writable } from "svelte/store";

export let nodeStructures = writable<NodeStructure[]>([]);

export interface NodeStructure {
    Type: string;
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
    DefaultValue: string;
}
