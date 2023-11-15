import { execa } from "execa";
import { renameSync, mkdirSync, existsSync, copyFileSync, rmdirSync, readdirSync, readFileSync, writeFileSync } from "fs";
import { join } from "path";

let extension = "";
let arch = "";
switch (process.platform) {
    case "win32":
        extension = ".exe";
        arch = "win-x64";
        break;
    case "darwin":
        extension = "";
        arch = "osx-arm64"; // TODO: Detect M1 vs Intel
        break;
    default:
        throw new Error(`Unsupported platform: ${process.platform}`);
}

const source = `src-csharp/bin/Release/net7.0/${arch}/publish`;
const targetExe = "src-tauri/binaries";
const targetDll = "src-tauri";

async function main() {
    // Create the target directory
    if (existsSync(targetExe)) {
        console.log(`Removing ${targetExe}`);
        rmdirSync(targetExe, { recursive: true });
    }

    console.log(`Creating ${targetExe}`);
    mkdirSync(targetExe, { recursive: true });

    // Build the C# project
    console.log("Building C# project");
    await execa("dotnet", ["clean", "src-csharp", "-c", "Release"]);
    await execa("dotnet", ["publish", "src-csharp", "-c", "Release", "-f", "net7.0", "--self-contained", "true", "/p:PublishSingleFile=true"]);

    // Get the target triple
    const rustInfo = (await execa("rustc", ["-vV"])).stdout;
    const targetTriple = /host: (\S+)/g.exec(rustInfo)[1];
    if (!targetTriple) {
        console.error("Failed to determine platform target triple");
    }

    // Log the target triple
    console.log(`Target triple: ${targetTriple}`);

    // Rename the executable
    console.log(`Renaming ${source}/Vla${extension} to ${source}/csharp-${targetTriple}${extension}`);
    renameSync(`${source}/Vla${extension}`, `${source}/csharp-${targetTriple}${extension}`);

    const files = readdirSync(source);
    let dllFiles = [];
    for (const file of files) {
        if (file.endsWith(".pdb")) continue;

        let sourceFile = join(source, file);
        let targetFile = join(targetExe, file);

        if (file.endsWith(".dll") || file.endsWith(".dylib")) {
            targetFile = join(targetDll, file);
            dllFiles.push(targetFile.replace("src-tauri", "."));
        }

        console.log(`Copying ${sourceFile} to ${targetFile}`);

        copyFileSync(sourceFile, targetFile);
    }

    console.log("Modifying tauri.conf.json");
    // Read the tauri.conf.json file in src-tauri/tauri.conf.json
    const tauriConf = JSON.parse(readFileSync("src-tauri/tauri.conf.json", "utf8"));

    tauriConf["tauri"]["bundle"]["resources"] = dllFiles;

    // Write the tauri.conf.json file in src-tauri/tauri.conf.json
    writeFileSync("src-tauri/tauri.conf.json", JSON.stringify(tauriConf, null, 4), "utf8");
}

main().catch((e) => {
    throw e;
});
