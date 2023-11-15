<script lang="ts">
    import type { ClassicPreset } from "rete";
    import { types } from "../lib/nodes";

    export let data: ClassicPreset.Socket;
</script>

<div class="socket" style={`border-color: rgb(${$types.find((x) => x.Name == data.name)?.Color})`} title={data.name} />

<style lang="scss" scoped>
    @use "sass:math";

    $socket-size: 16px;

    .socket {
        display: inline-block;
        cursor: pointer;
        border-radius: 100%;
        width: $socket-size;
        height: $socket-size;
        vertical-align: middle;
        background: #000000;
        border: 3px solid black;
        z-index: 2;
        box-sizing: border-box;

        &:hover {
            background: #ddd;
        }
    }
</style>
