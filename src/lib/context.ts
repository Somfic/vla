import { get } from "svelte/store";
import { structures, instances } from "./nodes";
import Fuse from "fuse.js";

export function addNode(query: string): ContextResult[] {
    const search = new Fuse(get(structures), {
        keys: ["NodeType"],
        threshold: 0.4,
    });

    const result = search.search(query);

    return result.map((r) => {
        return {
            name: r.item.NodeType.split(",")[0].split(".").slice(-1)[0].replace("Node", ""),
            context: r.item.NodeType.split(".")[0],
            action: () => {
                instances.update((i) => {
                    i.push({
                        Id: generateGuid(),
                        NodeType: r.item.NodeType,
                        Metadata: {
                            Position: {
                                X: 0,
                                Y: 0,
                                x: 0,
                                y: 0,
                            },
                        },
                        Properties: r.item.Properties.map((p) => {
                            return {
                                Name: p.Name,
                                Type: p.Type,
                                Value: p.DefaultValue,
                                DefaultValue: p.DefaultValue,
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
    context?: string;
    action: () => void;
}
