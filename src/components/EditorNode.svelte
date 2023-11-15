<script lang="ts">
    import type { ClassicScheme, SvelteArea2D } from "rete-svelte-plugin";
    import { Ref } from "rete-svelte-plugin";
    type NodeExtraData = { width?: number; height?: number };

    function sortByIndex<K, I extends undefined | { index?: number }>(entries: [K, I][]) {
        entries.sort((a, b) => {
            const ai = (a[1] && a[1].index) || 0;
            const bi = (b[1] && b[1].index) || 0;
            return ai - bi;
        });
        return entries as [K, Exclude<I, undefined>][];
    }

    export let data: ClassicScheme["Node"] & NodeExtraData;
    export let emit: (props: SvelteArea2D<ClassicScheme>) => void;

    $: width = Number.isFinite(data.width) ? `${data.width}px` : "";
    $: height = Number.isFinite(data.height) ? `${data.height}px` : "";

    $: inputs = sortByIndex(Object.entries(data.inputs));
    $: controls = sortByIndex(Object.entries(data.controls));
    $: outputs = sortByIndex(Object.entries(data.outputs));
    function any<T>(arg: T): any {
        return arg;
    }
</script>

<div class="node {data.selected ? 'selected' : ''}" style:width style:height data-testid="node">
    <div class="title" data-testid="title">{data.label}</div>

    <div class="controls">
        <!-- Controls -->
        {#each controls as [key, control]}
            <Ref
                class="control"
                data-testid={"control-" + key}
                init={(element) =>
                    emit({
                        type: "render",
                        data: {
                            type: "control",
                            element,
                            payload: control,
                        },
                    })}
                unmount={(ref) => emit({ type: "unmount", data: { element: ref } })}
            />
        {/each}
    </div>

    <div class="inout">
        {#if inputs.length > 0}
            <div class="inputs">
                <!-- Inputs -->
                {#each inputs as [key, input]}
                    <div class="input" data-testid="'input-'+key">
                        <Ref
                            class="input-socket"
                            data-testid="input-socket"
                            init={(element) =>
                                emit({
                                    type: "render",
                                    data: {
                                        type: "socket",
                                        side: "input",
                                        key,
                                        nodeId: data.id,
                                        element,
                                        payload: input.socket,
                                    },
                                })}
                            unmount={(ref) => emit({ type: "unmount", data: { element: ref } })}
                        />
                        {#if !input.control || !input.showControl}
                            <div class="input-title" data-testid="input-title">
                                {input.label || ""}
                            </div>
                        {/if}
                        {#if input.control && input.showControl}
                            <Ref
                                class="input-control"
                                data-testid="input-control"
                                init={(element) =>
                                    emit({
                                        type: "render",
                                        data: {
                                            type: "control",
                                            element,
                                            payload: any(input).control,
                                        },
                                    })}
                                unmount={(ref) => emit({ type: "unmount", data: { element: ref } })}
                            />
                        {/if}
                    </div>
                {/each}
            </div>
        {/if}
        {#if outputs.length > 0}
            <div class="outputs">
                <!-- Outputs -->
                {#each outputs as [key, output]}
                    <div class="output" data-testid="'output-'+key">
                        <div class="output-title" data-testid="output-title">
                            {output.label || ""}
                        </div>
                        <Ref
                            class="output-socket"
                            data-testid="output-socket"
                            init={(element) =>
                                emit({
                                    type: "render",
                                    data: {
                                        type: "socket",
                                        side: "output",
                                        key,
                                        nodeId: data.id,
                                        element,
                                        payload: output.socket,
                                    },
                                })}
                            unmount={(ref) => emit({ type: "unmount", data: { element: ref } })}
                        />
                    </div>
                {/each}
            </div>
        {/if}
    </div>
</div>

<style lang="scss">
    $node-width: 200px;
    $socket-margin: 6px;
    $socket-size: 16px;

    @use "sass:math";

    .node {
        background-color: #343434;
        min-width: 200px;
    }

    .title {
        background-color: #ad50a3;
        padding: 12px;
        font-weight: bold;
    }

    .controls {
        padding: 12px;
    }

    .inout {
        display: flex;

        .inputs,
        .outputs {
            flex-grow: 1;
            display: flex;
            flex-direction: column;
            gap: 8px;
            padding-top: 16px;
            padding-bottom: 16px;
        }

        .outputs {
            background-color: #1f1f1f;
        }

        .input,
        .output {
            display: flex;
            align-items: center;
            gap: 10px;
        }

        .input {
            justify-content: left;
            margin-left: -5px;
        }

        .output {
            justify-content: right;
            margin-right: -5px;
        }
    }
</style>
