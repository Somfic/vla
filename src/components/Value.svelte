<script lang="ts">
    import type { TypeDefinition } from "../lib/nodes";
	export let type: TypeDefinition;
	export let value: any;
	export let readonly = false;
	export let input = false;
	export let output = false;
</script>

<div class="value" class:hasValue={value} class:readonly class:input class:output>
{#if type.htmlType == "text"}
    <input {readonly} bind:value type="text" placeholder={type.defaultValue} />
{:else if type.htmlType == "number"}
    <input {readonly} bind:value type="number" placeholder={type.defaultValue} />
{:else if type.htmlType == "select"}
	{#if readonly}
		<input {readonly} bind:value type="text" placeholder={type.defaultValue} />
	{:else}
		<select bind:value>
			{#each type.values as option}
				<option value={option.value}>{option.name}</option>
			{/each}
		</select>
	{/if}
{:else if type.htmlType == "checkbox"}
    <input {readonly} bind:checked={value} type="checkbox" />
{/if}
</div>

<style lang="scss">
	@import "../theme.scss";

	.value {
		display: flex;
		transition: all ease 200ms;

        &.input {
            text-align: left;
        }

        &.output input {
            text-align: right;
			margin-right: 0.5rem;
        }

		&.readonly {
			opacity: 0;
        }

		&.hasValue {
			opacity: 1;
		}

        input, select {
			all: unset;
			font-family: $font-mono;
			font-weight: bold;
			font-size: 0.8rem;
			height: 1rem;
			flex-grow: 1;
			width: 50px;
			border: 2px solid $border-color;
			border-radius: 10px;
			background-color: $background-dark;
			padding: 5px 5px;
			color: #999999;
        }

		select {
			width: 100%;
		}
    }
</style>