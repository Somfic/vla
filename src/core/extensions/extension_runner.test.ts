import { expect, test } from "vitest";
import { readFile } from "node:fs/promises";
import { ExtensionRunner } from "./extension_runner";
import { AppHandle } from "../../extensions/abstractions/abstractions";

test("detects extensions", async () => {
    const runner = new ExtensionRunner();
    const handle = new TestAppHandle();

    const extension_code = await readFile("src/extensions/test/dist/main.js", "utf-8");

    const extensions = runner.registerExtension(extension_code);

    await runner.start(handle);

    expect(extensions.length).toBe(1);
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
