<script lang="ts">
    import { get } from "svelte/store";
    import { result } from "../lib/nodes";

    export let id: string;
    export let input: boolean = false;
    export let output: boolean = false;

    let value: string | undefined = undefined;

    result.subscribe((r) => {
        if (r == undefined) return;
        if (r.values == undefined) return;

        value = r.values.find((v) => v.id == id)?.value;
    });
</script>

<div class="computed" class:input class:output class:value>
    {#if value}
        <p>{value}</p>
    {/if}
</div>

<style lang="scss">
    @import "../theme.scss";

    .computed {
        width: 50px;
        height: 1rem;
        border: 2px solid $border-color;
        border-radius: 5px;
        background-color: $background-dark;
        padding: 5px;
        opacity: 0;
        transition: opacity ease 200ms;
        display: flex;

        &.input p {
            text-align: left;
        }

        &.output p {
            text-align: right;
        }

        &.value {
            opacity: 1;
        }

        p {
            font-family: $font-mono;
            font-weight: bold;
            flex-grow: 1;
            margin: 0;
            color: #696969; // nice
            // hide overflow with ...
            overflow: hidden;
        }
    }
</style>
