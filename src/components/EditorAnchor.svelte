<script lang="ts">
    import { get } from "svelte/store";
    import { types, type Parameter } from "../lib/nodes";

    export let parameter: Parameter;
    export let linked: boolean;
    export let hovering: boolean;
    export let connecting: boolean;

    function getColorFromType(type: string) {
        return get(types).find((t) => t.Type === type.replace("&", ""))?.Color;
    }
</script>

<div class="anchor" class:linked class:hovering class:connecting style={`--type-color: rgba(${getColorFromType(parameter.Type)})`} />

<style lang="scss">
    .anchor {
        height: 10px;
        width: 10px;

        border-radius: 50%;

        background-color: black;
        border: 2px solid var(--type-color);

        &.connecting {
            background-color: var(--type-color);
        }

        &.linked {
            background-color: var(--type-color);
        }
    }
</style>
