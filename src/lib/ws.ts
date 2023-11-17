import { writable, type Writable } from "svelte/store";
import { types, structures, instances, connections, result, type WebResult } from "./nodes";

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
            case "NodesStructure":
                types.set(data["Types"]);
                structures.set(data["Nodes"]);
                break;

            case "Web":
                instances.set(data["Web"]["Instances"]);
                connections.set(data["Web"]["Connections"]);
                break;

            case "WebResult":
                result.set(data["Result"]);
                break;
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

export function sendMessage(message: any) {
    if (!ws) return;
    if (ws.readyState !== ws.OPEN) return console.log("Not open");

    ws.send(JSON.stringify(message));
}
