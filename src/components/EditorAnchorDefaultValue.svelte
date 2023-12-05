<script lang="ts">
    import { get } from "svelte/store";
    import { types, type ParameterStructure, typeToDefinition, type ParameterInstance, type NodeStructure } from "../lib/nodes";
    import Value from "./Value.svelte";

    export let structure: NodeStructure;
    export let parameter: ParameterInstance;
    export let linked: boolean;
    export let hovering: boolean;
    export let connecting: boolean;

    function handleClick(e: MouseEvent) {
        //e.stopImmediatePropagation();
        e.stopPropagation();
        //e.preventDefault();
    }

    $: parameterType = structure.inputs.find((i) => i.id == parameter.id)?.type.replace("&", "")!;
</script>

<div class="default-wrapper">
    <!-- svelte-ignore a11y-click-events-have-key-events -->
    <!-- svelte-ignore a11y-no-static-element-interactions -->
    <div class="default" class:linked class:hovering class:connecting on:mousedown={handleClick}>
        <Value type={typeToDefinition(parameterType)} bind:value={parameter.value} input />
    </div>
</div>

<style lang="scss">
    @import "../theme.scss";

    .default-wrapper {
        position: absolute;
        top: -0.5rem;
    }

    .default {
        display: inline-block;
        left: calc(-100% - 0.5rem);
        position: relative;
        opacity: 0.5;
        transition: all ease 200ms;

        &:hover {
            opacity: 1;
        }
    }
</style>
