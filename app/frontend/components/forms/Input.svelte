<script lang="ts">
	import type { ArgumentType } from "$lib/schema";

	let {
		id,
		value = $bindable(),
		type,
		enumValues,
		label,
		disabled,
		onchange,
	}: {
		id?: string;
		value: string | null | undefined;
		type: ArgumentType;
		label?: string;
		disabled?: boolean;
		enumValues?: string[];
		onchange?: (value: unknown) => void;
	} = $props();

	let rawValue = $state<unknown>(value ? JSON.parse(value) : null);

	$effect(() => {
		// value is always a JSON-encoded string on the wire (matches backend)
		value = JSON.stringify(rawValue);
	});
</script>

<div class={`input ${type} nodrag`} class:disabled class:labeled={!!label}>
	{#if label}
		<label for={id}>{label}</label>
	{/if}

	{#if type === "boolean"}
		<!-- svelte-ignore a11y_consider_explicit_label -->
		<button
			{id}
			{disabled}
			type="button"
			class="nodrag toggle"
			class:active={!!rawValue}
			onclick={() => {
				rawValue = !rawValue;
				onchange?.(rawValue);
			}}
		>
			<div class="toggle-slider"></div>
		</button>
	{:else if type === "number"}
		<input
			{id}
			{disabled}
			type="number"
			class="nodrag"
			bind:value={rawValue}
			onchange={() => onchange?.(rawValue)}
		/>
	{:else if type === "string"}
		<input
			{id}
			{disabled}
			type="text"
			class="nodrag"
			bind:value={rawValue}
			onchange={() => onchange?.(rawValue)}
		/>
	{:else if type === "enum" && enumValues}
		<select
			{id}
			{disabled}
			class="nodrag"
			bind:value={rawValue}
			onchange={() => onchange?.(rawValue)}
		>
			{#each enumValues as option}
				<option value={option} selected={option === rawValue}>
					{option}
				</option>
			{/each}
		</select>
	{/if}
</div>

<style lang="scss">
	.input {
		display: flex;
		flex-grow: 1;
		align-items: center;
		gap: 0.25rem;
		border: 1px solid var(--glow-border-color);
		border-radius: 999px;
		padding: 0.25rem 0.5rem;
		font-size: 0.75rem;
		color: var(--glow-text-secondary);
		background: var(--glow-bg-surface-element);
		transition:
			border-color var(--glow-dur-fast) var(--glow-ease-out),
			background var(--glow-dur-fast) var(--glow-ease-out);

		input,
		select {
			display: flex;
			flex-grow: 1;
			font-weight: 700;
			text-align: right;
			max-width: 80px;
			background: transparent;
			border: none;
			outline: none;
			color: inherit;
		}

		label {
			white-space: nowrap;
		}

		&.labeled {
			padding: 0.125rem 0.25rem;
		}

		&:not(.disabled) {
			cursor: pointer;

			input,
			select {
				color: var(--glow-text-primary);
			}

			&:hover,
			&:focus-within {
				border-color: var(--glow-primary);
			}
		}
	}

	.toggle {
		width: 32px;
		height: 16px;
		cursor: pointer;
		background-color: var(--glow-bg-surface-element);
		border: 1px solid var(--glow-border-color);
		border-radius: 8px;
		position: relative;
		padding: 0;
		transition: all var(--glow-dur-fast) var(--glow-ease-out);

		.toggle-slider {
			width: 12px;
			height: 12px;
			background-color: var(--glow-fg);
			border-radius: 50%;
			position: absolute;
			top: 50%;
			left: 2px;
			transform: translateY(-50%);
			transition: all var(--glow-dur-fast) var(--glow-ease-out);
		}

		&.active {
			background-color: var(--glow-primary);
			border-color: var(--glow-primary);

			.toggle-slider {
				left: calc(100% - 14px);
				background-color: white;
			}
		}

		&:hover {
			border-color: var(--glow-primary);
		}
	}
</style>
