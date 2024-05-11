import { compile, compileFromFile } from "json-schema-to-typescript";
import { glob } from "glob";
import { writeFileSync } from "fs";
import { resolve } from "path";

const files = glob.sync("src/*.schema.json");

console.log("Found", files.length, "files to process: ", files);

files.forEach(async (file) => {
    const ts = await compileFromFile(file);
    const tsFile = file.replace(".schema.json", ".d.ts").split("\\").pop();
    const path = resolve("src", "ui", "src", "lib", "models", tsFile);

    writeFileSync(path, ts);
});
