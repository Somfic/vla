import { type AppHandle, Extension } from "@somfic/vla-extension";

export class TestExtension extends Extension {
    async on_start(handle: AppHandle) {
        handle.log("test_extension");
    }

    async on_stop(handle: AppHandle): Promise<void> {}
}

export class TestExtension2 {
    async on_start(handle: AppHandle) {
        handle.log("test_extension2");
    }

    async on_stop(handle: AppHandle): Promise<void> {}
}
