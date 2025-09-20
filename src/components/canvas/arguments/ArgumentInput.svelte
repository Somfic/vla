<script lang="ts">
    import type { NodeData, BrickArgument } from "$lib/core";
    import { saveNodeChanges } from "$lib/api";

    interface Props {
        argument: BrickArgument;
        data: NodeData;
    }

    let { argument, data }: Props = $props();

    function getValue(argumentId: string, type: string) {
        const value = data.arguments[argumentId];
        if (value === undefined) {
            return type === "Boolean" ? false : type === "Number" ? 0 : "";
        }
        try {
            return JSON.parse(value);
        } catch {
            return type === "Boolean" ? false : type === "Number" ? 0 : "";
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
    <label for={argument.id}>{argument.label}</label>

    {#if argument.type === "Boolean"}
        <input
            type="checkbox"
            id={argument.id}
            class="nodrag"
            checked={getValue(argument.id, "Boolean")}
            onchange={(e) => setValue(argument.id, e.currentTarget.checked)}
        />
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
    {/if}
</div>

<style lang="scss">
    @import "$styles/theme";

    .argument {
        display: flex;
        gap: $gap;
        align-items: center;
        justify-content: space-between;

        input[type="checkbox"] {
            width: 16px;
            height: 16px;
            cursor: pointer;
            appearance: none;
            background-color: $background-secondary;
            border: 2px solid $border-color;
            border-radius: 3px;
            position: relative;
            margin: 0;

            &:checked {
                background-color: $primary;
                border-color: $primary;

                &::after {
                    content: "✓";
                    position: absolute;
                    top: 50%;
                    left: 50%;
                    transform: translate(-50%, -50%);
                    color: $foreground;
                    font-size: 12px;
                    font-weight: bold;
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
