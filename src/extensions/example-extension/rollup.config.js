import typescript from "@rollup/plugin-typescript";
import commonjs from "@rollup/plugin-commonjs";
import noderesolve from "@rollup/plugin-node-resolve";
import nodePolyfills from "rollup-plugin-polyfill-node";

/** @type {import('rollup').RollupOptions} */
export default {
    input: "src/index.ts",
    output: {
        dir: "output",
        format: "cjs",
    },
    plugins: [typescript(), commonjs(), noderesolve(), nodePolyfills()],
};
