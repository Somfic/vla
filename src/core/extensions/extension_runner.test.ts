import { expect, test } from "vitest";
import { readFile } from "node:fs/promises";
import { ExtensionRunner } from "./extension_runner";
import { AppHandle } from "./extension";

test("runs on_start", async () => {
    const runner = new ExtensionRunner();
    const handle = new TestAppHandle();

    const extension_code = await readFile("src/extensions/example-extension/output/main.js", "utf-8");

    const extensions = runner.registerExtension(extension_code);

    expect(extensions.length).toBe(1);

    await runner.start(handle);

    expect(handle.getMessages()).toEqual(["on_start"]);
});

class TestAppHandle implements AppHandle {
    messages: string[] = [];

    log(message: string): void {
        this.messages.push(message);
    }

    getMessages(): string[] {
        return this.messages;
    }
}
