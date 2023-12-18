import { get } from "svelte/store";
import { workspace } from "./state.svelte";

export function getDefaultValueForType(type: string): any {
    return get(workspace)?.types.find((t) => t.type == type)?.defaultValue;
}

export function generateGuid(): string {
    return "xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx".replace(/[xy]/g, function (c) {
        let r = (Math.random() * 16) | 0,
            v = c == "x" ? r : (r & 0x3) | 0x8;
        return v.toString(16);
    });
}
