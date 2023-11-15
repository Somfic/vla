import { NodeEditor, type GetSchemes, ClassicPreset } from "rete";
import { AreaPlugin, AreaExtensions, Drag } from "rete-area-plugin";
import { ConnectionPlugin, Presets as ConnectionPresets } from "rete-connection-plugin";
import { SveltePlugin, Presets, type SvelteArea2D } from "rete-svelte-plugin";
import type { NodeStructure } from "./nodes";

type Schemes = GetSchemes<ClassicPreset.Node, ClassicPreset.Connection<ClassicPreset.Node, ClassicPreset.Node>>;
type AreaExtra = SvelteArea2D<Schemes>;

export async function createEditor(container: HTMLElement, structures: NodeStructure[]) {
    const socket = new ClassicPreset.Socket("socket");

    const editor = new NodeEditor<Schemes>();
    const area = new AreaPlugin<Schemes, AreaExtra>(container);
    const connection = new ConnectionPlugin<Schemes, AreaExtra>();
    const render = new SveltePlugin<Schemes, AreaExtra>();

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

    render.addPreset(Presets.classic.setup());

    connection.addPreset(ConnectionPresets.classic.setup());

    editor.use(area);
    area.use(connection);
    area.use(render);

    AreaExtensions.simpleNodesOrder(area);

    for (const structure of structures) {
        const node = new ClassicPreset.Node(structure.Type);

        for (const property of structure.Properties) {
            node.addControl(property.Name, new ClassicPreset.InputControl("text", { initial: property.DefaultValue }));
        }

        for (const input of structure.Inputs) {
            node.addInput(input.Name, new ClassicPreset.Input(socket));
        }

        for (const output of structure.Outputs) {
            node.addOutput(output.Name, new ClassicPreset.Output(socket));
        }

        await editor.addNode(node);

        await area.translate(node.id, { x: 0, y: 0 });
    }

    setTimeout(() => {
        // wait until nodes rendered because they dont have predefined width and height
        AreaExtensions.zoomAt(area, editor.getNodes());
    }, 10);
    return {
        destroy: () => area.destroy(),
    };
}
