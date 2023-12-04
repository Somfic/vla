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
    <div class="property-name">{property.name}</div>
    <Value {type} bind:value={value} />
</div>

<style lang="scss">
    @import "../theme.scss";

    .property {
        display: flex;
        align-items: center;
        justify-content: space-between;

        margin: 1rem 15px;
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

            &[type="checkbox"] {
                width: 1rem;
                height: 1rem;
                margin: 0;
                padding: 0;
                appearance: none;
                background-color: #1f1f1f;
                border: 2px solid #424242;
                border-radius: 5px;
                transition: all ease 200ms;
                cursor: pointer;

                &:checked {
                    background-color: $accent;
                    border-color: $accent;
                }
            }
        }

        select {
            padding-right: 1.1rem;
            cursor: pointer;
        }
    }
</style>
