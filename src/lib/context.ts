import { get } from "svelte/store";
import { structures, instances, types } from "./nodes";
import Fuse from "fuse.js";

function getDefaultValueForType(type: string): any {
    return get(types).find((t) => t.type == type)?.defaultValue;
}

export function addNode(query: string): ContextResult[] {
    const search = new Fuse(get(structures), {
        keys: ["searchTerms"],
        threshold: 0.3,
    });

    const result = search.search(query);

    return result.map((r) => {
        return {
            name: r.item.name,
            sourcePlugin: r.item.nodeType.split(".")[0],
            category: r.item.category,
            action: () => {
                instances.update((i) => {
                    i.push({
                        id: generateGuid(),
                        nodeType: r.item.nodeType,
                        metadata: {
                            position: {
                                x: 0,
                                y: 0,
                            },
                        },
                        inputs: r.item.inputs.map((p) => {
                            console.log("Setting default value for input", p.name, p.defaultValue);
                            return {
                                id: p.id,
                                value: p.defaultValue,
                            };
                        }),
                        properties: r.item.properties.map((p) => {
                            return {
                                name: p.name,
                                type: p.type,
                                value: getDefaultValueForType(p.type),
                            };
                        }),
                    });
                    return i;
                });
            },
        };
    });
}

function generateGuid(): string {
    return "xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx".replace(/[xy]/g, function (c) {
        let r = (Math.random() * 16) | 0,
            v = c == "x" ? r : (r & 0x3) | 0x8;
        return v.toString(16);
    });
}

export interface ContextResult {
    name: string;
    category: string | undefined;
    sourcePlugin: string | undefined;
    action: () => void;
}
