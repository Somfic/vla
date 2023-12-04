<script lang="ts">
    import { get } from "svelte/store";
    import { result, type TypeDefinition } from "../lib/nodes";
    import Value from "./Value.svelte";

    export let id: string;
    export let input: boolean = false;
    export let output: boolean = false;
    export let type: TypeDefinition;

    let value: string | undefined = undefined;

    result.subscribe((r) => {
        if (r == undefined) return;
        if (r.values == undefined) return;

        value = r.values.find((v) => v.id == id)?.value;
    });
</script>

<div class="computed" class:input class:output class:value>
    <Value {type} bind:value readonly output />
</div>