<script lang="ts">
    import { type Type } from "../../lib/core";

    let {
        id,
        value = $bindable(),
        type,
        enumValues,
        disabled,
        onchange,
    }: {
        id?: string;
        value: string | null | undefined;
        type: Type;
        disabled?: boolean;
        enumValues?: string[];
        onchange?: (value: any) => void;
    } = $props();

    let rawValue = $state<any>(value ? JSON.parse(value) : null);

    $effect(() => {
        // value should always be a string, so we need to json encode (svelte 5)
        value = JSON.stringify(rawValue);
    });
</script>

{#if type === "boolean"}
    <!-- svelte-ignore a11y_consider_explicit_label -->
    <button
        {id}
        {disabled}
        type="button"
        class="nodrag toggle {rawValue ? 'active' : ''}"
        class:active={!!rawValue}
        onclick={() => {
            rawValue = !rawValue;
            onchange?.(rawValue);
        }}
    >
        <div class="toggle-slider"></div>
    </button>
{:else if type === "number"}
    <div class="number-input nodrag">
        <input
            {id}
            {disabled}
            type="number"
            class="nodrag"
            bind:value={rawValue}
            onchange={() => onchange?.(rawValue)}
        />
    </div>
{:else if type === "string"}
    <input
        {id}
        {disabled}
        type="text"
        class="nodrag text-input"
        bind:value={rawValue}
    />
{:else if type === "enum" && enumValues}
    <select
        {id}
        {disabled}
        class="nodrag text-input"
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

<style lang="scss">
    @import "$styles/theme";

    .toggle {
        width: 32px;
        height: 16px;
        cursor: pointer;
        background-color: $background-secondary;
        border: $border;
        border-radius: 8px;
        position: relative;
        padding: 0;
        transition: all 0.2s ease;

        .toggle-slider {
            width: 12px;
            height: 12px;
            background-color: $foreground;
            border-radius: 50%;
            position: absolute;
            top: 50%;
            left: 2px;
            transform: translateY(-50%);
            transition: all 0.2s ease;
        }

        &.active {
            background-color: $primary;
            border-color: $primary;

            .toggle-slider {
                left: calc(100% - 14px);
                background-color: white;
            }
        }

        &:hover {
            border-color: $primary;
        }
    }

    .text-input {
        background-color: $background-secondary;
        border: $border;
        border-radius: 3px;
        color: $foreground;
        padding: 4px 8px;
        font-size: 12px;
        min-width: 80px;

        &:focus {
            outline: none;
            border-color: $primary;
        }

        &:hover {
            border-color: $primary;
        }
    }

    .number-input {
        display: flex;
        align-items: center;
        gap: $gap;

        input {
            background-color: $background-secondary;
            border: $border;
            border-radius: 3px;
            color: $foreground;
            padding: 4px 8px;
            font-size: 12px;
            width: 60px;
            text-align: center;

            &:focus {
                outline: none;
                border-color: $primary;
            }

            &:hover {
                border-color: $primary;
            }
        }

        .number-controls {
            display: flex;
            flex-direction: column;
            gap: 1px;

            button {
                background-color: $background-secondary;
                border: $border;
                color: $foreground;
                width: 16px;
                height: 12px;
                font-size: 10px;
                font-weight: bold;
                cursor: pointer;
                display: flex;
                align-items: center;
                justify-content: center;
                border-radius: 2px;

                &:hover {
                    background-color: $primary;
                    border-color: $primary;
                }

                &:active {
                    transform: scale(0.95);
                }
            }
        }
    }
</style>
