import { get, writable } from "svelte/store";

export const menu = writable("");
export const menuResult = writable(null as any);
export const show = writable(false);

export function invokeMenu(id: string, callback: (e: any) => void) {
    menu.set(id);
    menuResult.set(null);
    show.set(true);

    // Wait until menuResult has been set
    const interval = setInterval(() => {
        if (get(menuResult) !== null) {
            clearInterval(interval);
            callback(get(menuResult));
            clearInterval(interval);
        }
    }, 1);
}
