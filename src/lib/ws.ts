import { get, writable, type Writable } from "svelte/store";
import { types, structures, instances, connections, result, type WebResult, runWeb } from "./nodes";

let ws: WebSocket = null as any;

export let hasConnected = writable(false);
export let messages: Writable<string[]> = writable([]);
export let partialRecognition: Writable<string> = writable("");
export let recognition: Writable<string> = writable("");
export let progress: Writable<Progress> = writable({ percentage: 0, label: "Initialising" });
export let isReady: Writable<boolean> = writable(false);

isReady.subscribe((r) => {
    if (r) {
        runWeb();
    }
});

export function startListening() {
    hasConnected.set(false);
    instances.set([]);
    connections.set([]);
    result.set({} as WebResult);
    progress.set({ percentage: 0, label: "Initialising" });
    isReady.set(false);

    console.log("Connecting to websocket...");
    ws = new WebSocket("ws://127.0.0.1:55155");

    ws.onopen = () => {
        console.log("Connected");
        hasConnected.set(true);
        sendMessage({ Id: "getweb" });
    };

    ws.onmessage = (e) => {
        messages.update((old) => [...old, e.data]);

        const message = JSON.parse(e.data) as SocketMessage;
        console.log("<", message);

        switch (message.type) {
            case "NodesStructure":
                types.set(message.data["types"]);
                structures.set(message.data["nodes"]);
                break;

            case "Web":
                instances.set(message.data["web"]["instances"]);
                connections.set(message.data["web"]["connections"]);
                break;

            case "WebResult":
                result.set(message.data["result"]);
                break;

            case "Progress":
                progress.set(message.data);
                break;

            case "ReadyState":
                isReady.set(message.data["ready"]);
                break;
        }
    };

    ws.onerror = (e) => {
        console.log("Error", e);
    };

    ws.onclose = (e) => {
        console.log("Closed", e);

        setTimeout(() => {
            startListening();
        }, 5000);
    };
}

export function sendMessage(message: any) {
    if (!ws) return;
    if (ws.readyState !== ws.OPEN) return console.log("Not open");

    console.log(">", message);

    ws.send(JSON.stringify(message));
}

export interface SocketMessage {
    type: string;
    data: any;
}

export interface Progress {
    percentage: number;
    label: string;
}
