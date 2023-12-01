<script lang="ts">
    import { get } from "svelte/store";
    import { types, type Parameter } from "../lib/nodes";

    export let input: boolean = false;
    export let output: boolean = false;
    export let parameter: Parameter;
    export let linked: boolean;
    export let hovering: boolean;
    export let connecting: boolean;

    $: typeDefinition = get(types).find((t) => t.type == parameter.type.replace("&", ""))!;
</script>

<div class="anchor" class:input class:output class:linked class:hovering class:connecting style={`--type-color: rgba(${typeDefinition.color})`}>
    <svg viewBox="0 0 100 100" xmlns="http://www.w3.org/2000/svg">
        {#if typeDefinition.shape == "circle"}
            <circle cx="50" cy="50" r="40" />
        {:else if typeDefinition.shape == "diamond"}
            <polygon points="49,1 99,49 49,99 1,49" />
        {/if}
    </svg>
</div>

<style lang="scss">
    @import "../theme.scss";

    .anchor {
        position: relative;
        width: 12px;
        height: 12px;
        filter: drop-shadow(0px 0px 0px var(--type-color));

        &.input {
            margin-left: -5px;
        }

        &.output {
            margin-right: -5px;
        }

        &.connecting,
        &.linked,
        &:hover {
            svg {
                fill: var(--type-color);
            }
        }

        &.linked {
            filter: drop-shadow(0px 0px 5px var(--type-color));
        }

        svg {
            position: absolute;
            top: 0;
            left: 0;
            width: 100%;
            height: 100%;
            fill: $background;
            stroke: var(--type-color);
            stroke-width: 10px;

            transition: all ease 200ms;
        }
    }
</style>
