<script lang="ts">
    import { get } from "svelte/store";
    import type { NodeStructure, PropertyStructure, PropertyInstance } from "../lib/nodes";
    import { types } from "../lib/nodes";
    import Value from "./Value.svelte";

    export let property: PropertyStructure;
    export let value: any;

    $: type = get(types).find((x) => x.type == property.type)!;
</script>

<div class="property">
    <div class="name">{property.name}</div>
    <Value {type} bind:value />
</div>

<style lang="scss">
    @import "../theme.scss";

    .property {
        display: flex;
        align-items: center;
        justify-content: space-between;

        margin: 1rem 15px;
        gap: 6px;
    }
</style>
