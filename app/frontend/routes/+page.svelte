<script>
    import { api } from "$lib/schema";
    import { Button, Card, Data, Field, Page } from "glow";
    import { onMount } from "svelte";

    let bricks = $state(api.bricks.getBricks());

    let clock = $state("—");
    onMount(() => {
        const unlisten = api.clockEvents.onTick((tick) => {
            clock = tick.utc;
        });
        return () => unlisten.then((stop) => stop());
    });
</script>

<Page title="Test" layout="contained">
    <Card title="Clock" subtitle="Backend event, every second (UTC)">
        {clock}
    </Card>
    {#await bricks then bricks}
        {#each bricks as brick}
            <Card title={brick.label} subtitle={brick.description}>
                {#each brick.inputs as input}
                    <Field label={input.label} hint={input.type}>
                        {input.id}
                    </Field>
                {/each}
            </Card>
        {/each}
    {/await}
</Page>
