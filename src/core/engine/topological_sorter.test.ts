import { expect, test } from "vitest";
import { sort } from "./topological_sorter";

test("looping data", () => {
    test_topological_result(
        [
            ["a", "b"],
            ["b", "c"],
            ["c", "d"],
            ["d", "e"],
            ["e", "a"],
        ],
        // Since this is a loop we'll provide an expected result.
        // Automatic mode will expect 'e' to be before 'a',
        // which is an infinite loop. We must have a starting point.
        ["a", "b", "c", "d", "e"]
    );
});

test("simple data", () => {
    test_topological_result([
        ["a", "b"],
        ["b", "c"],
        ["c", "d"],
        ["d", "e"],
    ]);
});

test("multiple roots", () => {
    test_topological_result([
        ["a", "b"],
        ["a", "c"],
        ["b", "d"],
        ["b", "e"],
        ["c", "d"],
        ["d", "e"],
    ]);
});

test("complex", () => {
    test_topological_result([
        ["f", "a"],
        ["f", "c"],
        ["e", "a"],
        ["e", "b"],
        ["c", "d"],
        ["d", "b"],
    ]);
});

function test_topological_result(input: string[][], exptected: string[] | undefined = undefined) {
    const result = sort(input);

    if (exptected) {
        return expect(result).toEqual(exptected);
    }

    // Automatic mode
    input.forEach(([a, b]) => {
        expect(result, `expected '${a}' to be in the result`).toContain(a);
        expect(result, `expected '${b}' to be in the result`).toContain(b);
        expect(result.indexOf(a), `expected '${a}' to be before '${b}'`).toBeLessThan(result.indexOf(b));
    });
}
