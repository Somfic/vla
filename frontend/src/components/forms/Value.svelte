<script lang="ts">
    import Input from "$components/forms/Input.svelte";

    let { json }: { json: string | null | undefined } = $props();
</script>

<p>{json}</p>
