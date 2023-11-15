import { writable, type Writable } from "svelte/store";
import { nodeStructures } from "./nodes";

let ws: WebSocket = null as any;

export let hasConnected = writable(false);
export let messages: Writable<string[]> = writable([]);
export let partialRecognition: Writable<string> = writable("");
export let recognition: Writable<string> = writable("");

export function startListening() {
    console.log("Connecting to websocket...");
    ws = new WebSocket("ws://127.0.0.1:55155");

    ws.onopen = () => {
        console.log("Connected");
        hasConnected.set(true);
    };

    ws.onmessage = (e) => {
        console.log("Message", e);
        messages.update((old) => [...old, e.data]);

        const data = JSON.parse(e.data);

        switch (data["Id"]) {
            case "NodesStructureMessage":
                nodeStructures.set(data["Nodes"]);
        }
    };

    ws.onerror = (e) => {
        console.log("Error", e);
    };

    ws.onclose = (e) => {
        console.log("Closed", e);
        hasConnected.set(false);

        setTimeout(() => {
            startListening();
        }, 5000);
    };
}
