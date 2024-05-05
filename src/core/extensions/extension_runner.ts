import { Script, createContext } from "vm";
import { AppHandle, Extension, ExtensionMetadata } from "../../extensions/abstractions/abstractions";

export class ExtensionRunner {
    private extensions: Extension[] = [];

    constructor() {}
    registerExtension(extension_code: string) {
        const context = createContext({ exports: {} });

        const script = new Script(extension_code);

        script.runInContext(context);

        const exports = context.exports;

        // Find all the exported classes that implement Extension
        for (const key in exports) {
            const instance = new exports[key]() as Extension;

            if (!instance) continue;

            if (!instance.metadata || !instance.on_start || !instance.on_stop) {
                continue;
            }

            this.extensions.push(instance);
        }

        return this.extensions;
    }

    async start(handle: AppHandle) {
        for (const extension of this.extensions) {
            await extension.on_start(handle);
        }
    }

    async stop(handle: AppHandle) {
        for (const extension of this.extensions) {
            if (extension.on_stop) {
                await extension?.on_stop(handle);
            }
        }
    }

    minimal_extension: Extension = {
        metadata: function (): ExtensionMetadata {
            return {
                name: "",
                version: "",
                description: "",
            };
        },
        on_start: async (_handle: AppHandle) => {},
        on_stop: async (_handle: AppHandle) => {},
    };
}
