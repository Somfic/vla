import { expect, test } from "vitest";
import { HELLO } from ".";

test("HELLO returns hello world", () => {
    expect(HELLO).toBe("Hello, World!");
});
