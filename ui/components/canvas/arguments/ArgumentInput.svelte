<script lang="ts">
    import type { NodeData, BrickArgument, Brick } from "$lib/core";
    import { saveNodeChanges } from "$lib/api";

    interface Props {
        argument: BrickArgument;
        data: NodeData;
    }

    let { argument, data }: Props = $props();

    function getDefaultValue(type: string) {
        switch (type) {
            case "Boolean":
                return false;
            case "Number":
                return 0;
            case "String":
                return "";
            default:
                return null;
        }
    }

    function getValue(argumentId: string, type: string) {
        const value = data.arguments[argumentId];
        if (value === undefined) {
            return getDefaultValue(type);
        }
        try {
            return JSON.parse(value);
        } catch {
            return getDefaultValue(type);
        }
    }

    function setValue(argumentId: string, value: any) {
        data.arguments[argumentId] = JSON.stringify(value);
        saveNodeChanges();
    }

    let intervalId: number | null = null;
    let timeoutId: number | null = null;

    function startContinuousChange(argumentId: string, increment: number) {
        stopContinuousChange();

        timeoutId = window.setTimeout(() => {
            intervalId = window.setInterval(() => {
                setValue(
                    argumentId,
                    getValue(argumentId, "Number") + increment,
                );
            }, 100); // change speed
        }, 100); // initial delay
    }

    function stopContinuousChange() {
        if (timeoutId) {
            clearTimeout(timeoutId);
            timeoutId = null;
        }
        if (intervalId) {
            clearInterval(intervalId);
            intervalId = null;
        }
    }
</script>

<div class="argument">
    <label for={argument.id}>{@html argument.label}</label>

    {#if argument.type === "Boolean"}
        <button
            type="button"
            id={argument.id}
            class="nodrag toggle {getValue(argument.id, 'Boolean')
                ? 'active'
                : ''}"
            onclick={() =>
                setValue(argument.id, !getValue(argument.id, "Boolean"))}
        >
            <div class="toggle-slider"></div>
        </button>
    {:else if argument.type === "Number"}
        <div class="number-input nodrag">
            <input
                type="text"
                id={argument.id}
                class="nodrag"
                value={getValue(argument.id, "Number")}
                onchange={(e) => {
                    const value = e.currentTarget.value;
                    if (!isNaN(Number(value)) || value === "") {
                        setValue(argument.id, parseFloat(value) || 0);
                    }
                }}
            />
            <div class="number-controls">
                <button
                    type="button"
                    class="nodrag"
                    onclick={() =>
                        setValue(
                            argument.id,
                            getValue(argument.id, "Number") + 1,
                        )}
                    onmousedown={() => startContinuousChange(argument.id, 1)}
                    onmouseup={stopContinuousChange}
                    onmouseleave={stopContinuousChange}>+</button
                >
                <button
                    type="button"
                    class="nodrag"
                    onclick={() =>
                        setValue(
                            argument.id,
                            getValue(argument.id, "Number") - 1,
                        )}
                    onmousedown={() => startContinuousChange(argument.id, -1)}
                    onmouseup={stopContinuousChange}
                    onmouseleave={stopContinuousChange}>-</button
                >
            </div>
        </div>
    {:else if argument.type === "String"}
        <input
            type="text"
            id={argument.id}
            class="nodrag text-input"
            value={getValue(argument.id, "String")}
            onchange={(e) => setValue(argument.id, e.currentTarget.value)}
        />
    {:else if argument.type === "Enum" && argument.enum_options}
        <select
            id={argument.id}
            class="nodrag text-input"
            value={getValue(argument.id, "String")}
            onchange={(e) => setValue(argument.id, e.currentTarget.value)}
        >
            {#each argument.enum_options as option}
                <option
                    value={option}
                    selected={option === getValue(argument.id, "String")}
                >
                    {option}
                </option>
            {/each}
        </select>
    {/if}
</div>

<style lang="scss">
    @import "$styles/theme";

    .argument {
        display: flex;
        gap: $gap;
        align-items: center;
        justify-content: space-between;

        .toggle {
            width: 32px;
            height: 16px;
            cursor: pointer;
            background-color: $background-secondary;
            border: 2px solid $border-color;
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
            border: 2px solid $border-color;
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

            input[type="text"] {
                background-color: $background-secondary;
                border: 2px solid $border-color;
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
                    border: 2px solid $border-color;
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
    }
</style>
