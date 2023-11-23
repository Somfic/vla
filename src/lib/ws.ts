import { writable, type Writable } from "svelte/store";
import { types, structures, instances, connections, result, type WebResult, runWeb } from "./nodes";

let ws: WebSocket = null as any;

export let hasConnected = writable(false);
export let messages: Writable<string[]> = writable([]);
export let partialRecognition: Writable<string> = writable("");
export let recognition: Writable<string> = writable("");
export let progress: Writable<Progress> = writable({ Percentage: 0, Label: "Initialising" });
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
    progress.set({ Percentage: 0, Label: "Initialising" });
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

        const data = JSON.parse(e.data);
        console.log(">", JSON.stringify(data));

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

            case "Progress":
                progress.set(data);
                break;

            case "ReadyState":
                isReady.set(data["Ready"]);
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

    ws.send(JSON.stringify(message));
}

export interface Progress {
    Percentage: number;
    Label: string;
}
