import { type AppHandle, Extension, ExtensionMetadata } from "../../abstractions/abstractions";

export class TestExtension implements Extension {
    metadata(): ExtensionMetadata {
        return {
            name: "test_extension",
            description: "A test extension",
            version: "1.0.0",
        };
    }

    public async on_start(handle: AppHandle) {
        handle.log("test_extension");
    }

    async on_stop(_handle: AppHandle): Promise<void> {}
}

export class IrrelevantExport {}
