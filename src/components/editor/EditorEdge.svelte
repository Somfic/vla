<script lang="ts">
    import { Edge, type WritableEdge } from "svelvet";
    import { structures, type ParameterStructure, typeToDefinition, instanceFromId } from "../../lib/nodes";
    import { get } from "svelte/store";
    import { blendColors } from "../../lib/color";

    let ref: SVGPathElement | undefined = undefined;
    let edge: WritableEdge;

    $: source = findParameter(edge?.source.id, false);
    $: target = findParameter(edge?.target.id, true);

    $: startColor = typeToDefinition(source?.type ?? "")?.color?.hex ?? "#ffffff";
    $: midwayColor = blendColors(startColor, stopColor, 0.5);
    $: stopColor = typeToDefinition(target?.type ?? "")?.color?.hex ?? "#ffffff";

    $: gradientName = `gradient-${edge?.target.id ?? "default"}`;

    function findParameter(id: string | null, isInput: boolean): ParameterStructure | undefined {
        if (id == null) return undefined;

        let parameterId = id.split("/")[0].substring(2); // remove "A-"
        let instanceId = id.split("/")[1].substring(2); // remove "N-"

        let instance = instanceFromId(instanceId);
        let structure = get(structures).find((s) => s.nodeType == instance?.nodeType);

        if (isInput) {
            return structure?.inputs.find((p) => p.id == parameterId);
        } else {
            return structure?.outputs.find((p) => p.id == parameterId);
        }
    }
</script>

<Edge bind:edge let:path let:destroy edgeClick={() => alert("Edge clicked")}>
    <!-- Define a gradient that goes from the source color to the target color -->
    <defs>
        <linearGradient id={gradientName} x1="0%" y1="0%" x2="100%" y2="0%">
            <stop offset="0%" stop-color={startColor} />
            <stop offset="50%" stop-color={midwayColor} />
            <stop offset="100%" stop-color={stopColor} />
        </linearGradient>
    </defs>

    <path bind:this={ref} d={path} style:--gradient-name={`url(#${gradientName})`} />
</Edge>

<style lang="scss">
    path {
        // Gradient fill based on the source and target colors
        stroke: var(--gradient-name);
        stroke-width: 4px;
        z-index: 1;
    }
</style>
