<script lang="ts">
    import { get } from "svelte/store";
    import type { NodeStructure, Property, PropertyInstance } from "../lib/nodes";
    import { types } from "../lib/nodes";

    export let property: Property;
    export let structure: NodeStructure;
    export let value: string;

    $: type = get(types).find((x) => x.type == property.type)!;
    $: structureProperty = structure.properties.find((p) => p.name == property.name)!;
</script>

<div class="property">
    <div class="property-name">{property.name}</div>
    {#if structureProperty.htmlType == "text"}
        <input tabindex="-1" bind:value type="text" placeholder={structureProperty.defaultValue} />
    {:else if structureProperty.htmlType == "number"}
        <input tabindex="-1" bind:value type="number" placeholder={structureProperty.defaultValue} />
    {:else if structureProperty.htmlType == "select"}
        <select tabindex="-1" bind:value>
            {#each type.values as option}
                <option value={option.value}>{option.name}</option>
            {/each}
        </select>
    {/if}
</div>

<style lang="scss">
    @import "../theme.scss";

    .property {
        display: flex;
        align-items: center;
        justify-content: space-between;

        padding: 15px;
        gap: 6px;

        input, select {
            all: unset;
            max-width: 80px;
            text-align: right;
            height: 1rem;
            border: 2px solid #424242;
            border-radius: 5px;
            background-color: #1f1f1f;
            padding: 5px;
            color: #696969; // nice
            transition: all ease 200ms;

            &:focus {
                border-color: $accent;
                color: white;
            }
        }

        select {
            padding-right: 1.1rem;
        }
    }
</style>
