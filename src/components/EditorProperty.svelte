<script lang="ts">
    import type { Property } from "../lib/nodes";

    export let property: Property;
</script>

<div class="property">
    <div class="property-name">{property.Name}</div>
    <input type={property.HtmlType} placeholder={property.DefaultValue} />
</div>

<style lang="scss">
    .property {
        display: flex;
        align-items: center;
    }
</style>
