<script lang="ts">
    import { Edge } from "svelvet";

	let ref: SVGPathElement | undefined = undefined;
</script>

<Edge let:path let:destroy edgeClick="{() => alert('Edge clicked')}" step>
	<path bind:this={ref} d={path} />
</Edge>

<style lang="scss">
	path {
		stroke: #696969;
		stroke-width: 3px;
		z-index: 1;
	}
</style>