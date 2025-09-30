<script lang="ts">
    import type { NodeData, BrickArgument, Brick } from "$lib/core";
    import { saveNodeChanges } from "$lib/api";
    import Input from "$components/forms/Input.svelte";

    interface Props {
        argument: BrickArgument;
        data: NodeData;
    }

    let { argument, data }: Props = $props();
</script>

<div class="argument">
    <Input
        id={argument.id}
        type={argument.type}
        label={argument.label}
        bind:value={data.arguments[argument.id]!}
        onchange={() => saveNodeChanges()}
    />
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

            input[type="text"] {
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
    }
</style>
