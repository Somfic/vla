<script lang="ts">
	import { createEventDispatcher } from 'svelte';
	import type { TypeDefinition } from '$lib/nodes';

	export let type: TypeDefinition;
	export let value: any;
	export let readonly = false;
	export let input = false;
	export let output = false;

	const dispatch = createEventDispatcher();
</script>

<div class="value" class:hasValue={value != null} class:readonly class:input class:output>
	{#if type.htmlType == 'text'}
		<input
			{readonly}
			bind:value
			type="text"
			placeholder={type.defaultValue}
			on:change={() => dispatch('change')}
		/>
	{:else if type.htmlType == 'number' && !readonly}
		<input
			{readonly}
			bind:value
			type="number"
			placeholder={type.defaultValue}
			on:change={() => dispatch('change')}
		/>
	{:else if type.htmlType == 'number' && readonly}
		<input
			{readonly}
			bind:value
			type="text"
			placeholder={type.defaultValue}
			on:change={() => dispatch('change')}
		/>
	{:else if type.htmlType == 'select'}
		{#if readonly}
			<input
				{readonly}
				bind:value
				type="text"
				placeholder={type.defaultValue}
				on:change={() => dispatch('change')}
			/>
		{:else}
			<select bind:value on:change={() => dispatch('change')}>
				{#each type.values as option}
					<option value={option.value}>{option.name}</option>
				{/each}
			</select>
		{/if}
	{:else if type.htmlType == 'checkbox'}
		<input
			disabled={readonly}
			{readonly}
			bind:checked={value}
			type="checkbox"
			on:change={() => dispatch('change')}
		/>
	{/if}
</div>

<style lang="scss">
	@import '../../../styles/theme.scss';

	.value {
		display: flex;
		transition: all ease 200ms;

		&.input input {
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

		input,
		select {
			all: unset;
			cursor: pointer;
			font-weight: bold;
			font-size: 0.8rem;
			height: 1rem;
			flex-grow: 1;
			width: 50px;
			border: 2px solid $border-color;
			border-radius: 10px;
			background-color: $background-dark;
			padding: 3px 6px;
			color: $foreground-muted;
		}

		input[type='checkbox'] {
			width: calc(1rem + 10px);
			height: calc(1rem + 10px);
			margin: 0;
			padding: 0;
			appearance: none;
			background-color: $background-dark;
			border: 2px solid $border-color;
			border-radius: 10px;
			transition: all ease 200ms;
			cursor: pointer;

			&:checked {
				background-color: $accent;
				border-color: $accent;
			}
		}

		input {
			font-family: $font-mono;
		}

		select {
			width: calc(100% + 5rem);
			text-align: right;
			padding-right: 1rem;
		}
	}
</style>
