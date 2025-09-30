<script lang="ts">
    interface Props {
        value: any;
        type: "boolean" | "number" | "string" | "enum";
        enumOptions?: string[] | null;
        onchange?: (value: any) => void;
        id?: string;
        label?: string;
    }

    let { value, type, enumOptions, onchange, id, label }: Props = $props();

    function getDefaultValue(inputType: string) {
        switch (inputType) {
            case "boolean":
                return false;
            case "number":
                return 0;
            case "string":
                return "";
            default:
                return null;
        }
    }

    function getTypedValue(val: any, inputType: string) {
        if (val === undefined || val === null) {
            return getDefaultValue(inputType);
        }
        return val;
    }

    function handleChange(newValue: any) {
        if (onchange) {
            onchange(newValue);
        }
    }

    // Local reactive variables for input binding
    let displayValue = $derived(getTypedValue(value, type));
    let localStringValue = $state("");
    let localNumberValue = $state("");
    let localBooleanValue = $state(false);

    // Sync local values when displayValue changes
    $effect(() => {
        if (type === "string") {
            localStringValue = String(displayValue);
        } else if (type === "number") {
            localNumberValue = String(displayValue);
        } else if (type === "boolean") {
            localBooleanValue = Boolean(displayValue);
        }
    });

    let intervalId: number | null = null;
    let timeoutId: number | null = null;

    function startContinuousChange(increment: number) {
        stopContinuousChange();

        timeoutId = window.setTimeout(() => {
            intervalId = window.setInterval(() => {
                const currentValue = getTypedValue(value, "number");
                handleChange(currentValue + increment);
            }, 100);
        }, 100);
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

<div class="input-wrapper">
    {#if label}
        <label for={id}>{label}</label>
    {/if}

    {#if type === "boolean"}
        <button
            type="button"
            {id}
            class="nodrag toggle {localBooleanValue ? 'active' : ''}"
            onclick={() => {
                localBooleanValue = !localBooleanValue;
                handleChange(localBooleanValue);
            }}
        >
            <div class="toggle-slider"></div>
        </button>
    {:else if type === "number"}
        <div class="number-input nodrag">
            <input
                type="text"
                {id}
                class="nodrag"
                bind:value={localNumberValue}
                oninput={(e) => {
                    const inputValue = e.currentTarget.value;
                    if (!isNaN(Number(inputValue)) || inputValue === "") {
                        handleChange(parseFloat(inputValue) || 0);
                    }
                }}
            />
            <!-- <div class="number-controls">
                <button
                    type="button"
                    class="nodrag"
                    onclick={() => handleChange(displayValue + 1)}
                    onmousedown={() => startContinuousChange(1)}
                    onmouseup={stopContinuousChange}
                    onmouseleave={stopContinuousChange}>+</button
                >
                <button
                    type="button"
                    class="nodrag"
                    onclick={() => handleChange(displayValue - 1)}
                    onmousedown={() => startContinuousChange(-1)}
                    onmouseup={stopContinuousChange}
                    onmouseleave={stopContinuousChange}>-</button
                >
            </div> -->
        </div>
    {:else if type === "string"}
        <input
            type="text"
            {id}
            class="nodrag text-input"
            bind:value={localStringValue}
            oninput={(e) => handleChange(e.currentTarget.value)}
        />
    {:else if type === "enum" && enumOptions}
        <select
            {id}
            class="nodrag text-input"
            value={displayValue}
            onchange={(e) => handleChange(e.currentTarget.value)}
        >
            {#each enumOptions as option}
                <option
                    value={option}
                    selected={option === displayValue}
                >
                    {option}
                </option>
            {/each}
        </select>
    {/if}
</div>

<style lang="scss">
    @import "$styles/theme";

    .input-wrapper {
        display: flex;
        gap: $gap;
        align-items: center;
        justify-content: space-between;
        height: 100%;

        .toggle {
            width: 2rem;
            height: 1rem;
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
                border: $border;
                border-radius: 3px;
                color: $foreground;
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
    }
</style>
