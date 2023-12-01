<script lang="ts">
    import { get } from "svelte/store";
    import type { NodeStructure, PropertyInstance } from "../lib/nodes";

    export let property: PropertyInstance;
    export let structure: NodeStructure;
    export let value: string;

    $: structureProperty = structure.properties.find((p) => p.name == property.name)!;
</script>

<div class="property">
    <div class="property-name">{property.name}</div>
    {#if structureProperty.htmlType == "text"}
        <input tabindex="-1" bind:value type="text" placeholder={structureProperty.defaultValue} />
    {:else if structureProperty.htmlType == "number"}
        <input tabindex="-1" bind:value type="number" placeholder={structureProperty.defaultValue} />
    {/if}
</div>

<style lang="scss">
    .property {
        display: flex;
        align-items: center;

        padding: 15px;
        gap: 6px;

        input {
            all: unset;
            width: 80px;
            text-align: right;
            height: 1rem;
            border: 2px solid #424242;
            border-radius: 5px;
            background-color: #1f1f1f;
            padding: 5px;
            color: #696969; // nice
            transition: all ease 200ms;

            &:focus {
                border-color: #696969;
                color: white;
            }
        }
    }
</style>
