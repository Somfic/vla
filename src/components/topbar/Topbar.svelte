<div class="topbar">
    <div class="left"></div>
    <div class="center">
        <div class="name">
            <p class="workspace">Drafts</p>
            <div class="divider">/</div>
            <p class="name">Autosave</p>
        </div>
    </div>
    <div class="right"></div>
</div>

<style lang="scss">
    @import "../../theme.scss";

    .topbar {
        display: flex;
        align-items: center;
        justify-content: space-between;
        padding: 0 1rem;
        height: 3rem;
        background-color: $background-frosted;
        backdrop-filter: $filter-frosted;
        border-bottom: 1px solid $border-color;
        color: $foreground;
    }

    .name {
        display: flex;
        align-items: center;
        gap: 0.5rem;
    }
</style>
