import { NodeEditor, type GetSchemes, ClassicPreset } from "rete";
import { AreaPlugin, AreaExtensions, Drag } from "rete-area-plugin";
import { ConnectionPlugin, Presets as ConnectionPresets } from "rete-connection-plugin";
import { SveltePlugin, Presets, type SvelteArea2D } from "rete-svelte-plugin";
import type { NodeStructure, TypeDefinition } from "./nodes";
import EditorNode from "../components/EditorNode.svelte";
import EditorSocket from "../components/EditorSocket.svelte";
import EditorConnection from "../components/EditorConnection.svelte";

export let editor: NodeEditor<Schemes>;
export let area: AreaPlugin<Schemes, AreaExtra>;
export let connection: ConnectionPlugin<Schemes, AreaExtra>;
export let render: SveltePlugin<Schemes, AreaExtra>;

type Schemes = GetSchemes<ClassicPreset.Node, ClassicPreset.Connection<ClassicPreset.Node, ClassicPreset.Node>>;
type AreaExtra = SvelteArea2D<Schemes>;

let socketDictionary: { [key: string]: ClassicPreset.Socket } = {};

export async function createEditor(container: HTMLElement) {
    if (editor) return; // FIXME: Could be a problem if we want to create multiple editors or reload the editor

    editor = new NodeEditor<Schemes>();
    area = new AreaPlugin<Schemes, AreaExtra>(container);
    connection = new ConnectionPlugin<Schemes, AreaExtra>();
    render = new SveltePlugin<Schemes, AreaExtra>();

    AreaExtensions.selectableNodes(area, AreaExtensions.selector(), {
        accumulating: AreaExtensions.accumulateOnCtrl(),
    });

    area.area.setDragHandler(
        new Drag({
            down: (e) => {
                if (e.pointerType === "mouse" && e.button !== 1) return false;
                e.preventDefault();
                return true;
            },
            move: () => true,
        })
    );

    render.addPreset(
        Presets.classic.setup({
            customize: {
                node() {
                    return EditorNode;
                },
                socket() {
                    return EditorSocket;
                },
                connection() {
                    return EditorConnection;
                },
                control() {
                    return EditorControl;
                },
            },
        })
    );

    connection.addPreset(ConnectionPresets.classic.setup());

    editor.use(area);

    area.use(connection);
    area.use(render);

    AreaExtensions.simpleNodesOrder(area);

    setTimeout(() => {
        // wait until nodes rendered because they dont have predefined width and height
        AreaExtensions.zoomAt(area, editor.getNodes());
    }, 10);
    return {
        destroy: () => area.destroy(),
    };
}

export async function loadStructures(structures: NodeStructure[]) {
    editor.clear();

    for (const structure of structures) {
        const node = new ClassicPreset.Node(structure.Type.split(",")[0].split(".").slice(-1)[0].replace("Node", ""));

        for (const property of structure.Properties) {
            node.addControl(property.Name, new ClassicPreset.InputControl(property.HtmlType, { initial: property.DefaultValue }));
        }

        for (const input of structure.Inputs) {
            console.log(socketDictionary[input.Type]);
            const socket = socketDictionary[input.Type] || new ClassicPreset.Socket(input.Type);
            node.addInput(input.Name, new ClassicPreset.Input(socket, input.Name, false));
        }

        for (const output of structure.Outputs) {
            const socket = socketDictionary[output.Type] || new ClassicPreset.Socket(output.Type);
            node.addOutput(output.Name, new ClassicPreset.Output(socket, output.Name));
        }

        await editor.addNode(node);

        await area.translate(node.id, { x: 0, y: 0 });
    }
}

export async function loadTypes(types: TypeDefinition[]) {
    console.log("Loading types", types);
    for (const type of types) {
        socketDictionary[type.Type] = new ClassicPreset.Socket(type.Name);
    }
}
