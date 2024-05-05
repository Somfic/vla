import { Script, createContext } from "vm";
import { AppHandle, Extension } from "./extension";

export class ExtensionRunner {
    private extensions: Extension[] = [];

    constructor() {}
    registerExtension(extension_code: string) {
        const context = createContext({ exports: {} });

        extension_code = extension_code.replace('require("./extension");', "imports.extension");

        const script = new Script(extension_code);

        script.runInContext(context);

        const exports = context.exports;

        // Find all the exported classes that implement Extension
        for (const key in exports) {
            const instance = new exports[key]();
            const extensionMethods = Object.getOwnPropertyNames(Extension.prototype);
            const valueMethods = Object.getOwnPropertyNames(Object.getPrototypeOf(instance));
            const hasAllMethods = extensionMethods.every((method) => valueMethods.includes(method));
            if (hasAllMethods) {
                this.extensions.push(instance);
            }
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
}
