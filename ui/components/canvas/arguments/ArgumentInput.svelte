<script lang="ts">
    import type { NodeData, BrickArgument } from "$lib/core";
    import { saveNodeChanges } from "$lib/api";
    import Input from "$components/forms/Input.svelte";

    interface Props {
        argument: BrickArgument;
        data: NodeData;
    }

    let { argument, data }: Props = $props();
</script>

<div class="argument">
    <Input
        id={argument.id}
        type={argument.type}
        label={argument.label}
        bind:value={data.arguments[argument.id]!}
        onchange={() => saveNodeChanges()}
    />
</div>

<style lang="scss">
    @import "$styles/theme";

    .argument {
        display: flex;
        gap: $gap;
        align-items: center;
        justify-content: space-between;
    }
</style>
