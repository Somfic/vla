<script lang="ts">
    import { isReady } from "../lib/ws";
    import { progress } from "../lib/ws";

    let show = true;

    isReady.subscribe((r) => {
        if (r) {
            setTimeout(() => {
                show = false;
            }, 1000);
        } else {
            show = true;
        }
    });
</script>

{#if show}
    <div class="splashscreen-background" class:ready={$isReady}>
        <div class="splashscreen">
            <p class="label">{$progress.label}</p>
            <div class="progress">
                <div class="progress-bar" style={`width: ${$progress.percentage * 100}%`}></div>
            </div>
        </div>
    </div>
{/if}

<style lang="scss">
    @import "../theme.scss";

    .splashscreen-background {
        position: absolute;
        top: 0;
        left: 0;
        width: 100%;
        height: 100%;
        z-index: 200;
        background-color: rgba(0, 0, 0, 0.5);
        backdrop-filter: blur(5px);

        display: flex;
        align-items: center;
        justify-content: center;
        transition: 500ms ease all;

        &.ready {
            pointer-events: none;
            opacity: 0;
        }
    }

    .splashscreen {
        width: 300px;
        height: 400px;
        background-color: $background;
        border: 2px solid $border-color;
        border-radius: 10px;
        box-shadow: 0px 0px 100px 0px rgba(0, 0, 0, 1);
        padding: 20px;

        display: flex;
        flex-direction: column;
        align-items: center;
        justify-content: end;

        .label {
            font-size: 1rem;
            font-weight: bold;
            margin: 0;
            margin-bottom: 2rem;
        }

        .progress {
            width: 100%;
            height: 5px;
            background-color: $background-dark;
            border-radius: 10px;
            overflow: hidden;

            .progress-bar {
                height: 100%;
                background-color: $accent;
                transition: 1ms ease all;
            }
        }
    }
</style>
