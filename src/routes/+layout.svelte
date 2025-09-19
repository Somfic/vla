<script lang="ts">
    let { children } = $props();
</script>

{@render children()}

<style lang="scss">
    @import url("https://fonts.googleapis.com/css2?family=Zalando+Sans:ital,wght@0,200..900;1,200..900&display=swap");

    :global(*, *:before, *:after) {
        box-sizing: border-box;
    }

    :global(*) {
        font-family: "Zalando Sans", sans-serif;
        font-optical-sizing: auto;
        font-style: normal;
        font-variation-settings: "wdth" 100;
    }

    :global(
            html,
            body,
            div,
            span,
            object,
            iframe,
            figure,
            h1,
            h2,
            h3,
            h4,
            h5,
            h6,
            p,
            blockquote,
            pre,
            a,
            code,
            em,
            img,
            small,
            strike,
            strong,
            sub,
            sup,
            tt,
            b,
            u,
            i,
            ol,
            ul,
            li,
            fieldset,
            form,
            label,
            table,
            caption,
            tbody,
            tfoot,
            thead,
            tr,
            th,
            td,
            main,
            canvas,
            embed,
            footer,
            header,
            nav,
            section,
            video
        ) {
        margin: 0;
        padding: 0;
        border: 0;
        font-size: 100%;
        font: inherit;
        vertical-align: baseline;
        text-rendering: optimizeLegibility;
        -webkit-font-smoothing: antialiased;
        text-size-adjust: none;
    }

    :global(footer, header, nav, section, main) {
        display: block;
    }

    :global(body) {
        line-height: 1;
    }

    :global(ol, ul) {
        list-style: none;
    }

    :global(blockquote, q) {
        quotes: none;
    }

    :global(blockquote:before, blockquote:after, q:before, q:after) {
        content: "";
        content: none;
    }

    :global(table) {
        border-collapse: collapse;
        border-spacing: 0;
    }

    :global(input) {
        appearance: none;
        -webkit-appearance: none;
        border-radius: 0;
    }
</style>
