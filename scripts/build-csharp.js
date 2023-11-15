import { execa } from "execa";
import { renameSync, mkdirSync, existsSync, copyFileSync, rmdirSync, readdirSync } from "fs";
import { join } from "path";

let extension = ".exe";
if (process.platform !== "win32") {
    throw new Error("Unsupported platform");
}

const source = "src-csharp/bin/Release/net7.0/win-x64/publish";
const targetExe = "src-tauri/binaries";
const targetDll = "src-tauri";

async function main() {
    // Create the target directory
    if (existsSync(targetExe)) {
        rmdirSync(targetExe, { recursive: true });
    }

    mkdirSync(targetExe, { recursive: true });

    // Build the C# project
    await execa("dotnet", ["clean", "src-csharp", "-c", "Release"]);
    await execa("dotnet", ["publish", "src-csharp", "-c", "Release", "-f", "net7.0", "-r", "win-x64", "--self-contained", "true", "/p:PublishSingleFile=true"]); // "/p:PublishTrimmed=true"

    // Get the target triple
    const rustInfo = (await execa("rustc", ["-vV"])).stdout;
    const targetTriple = /host: (\S+)/g.exec(rustInfo)[1];
    if (!targetTriple) {
        console.error("Failed to determine platform target triple");
    }

    // Log the target triple
    console.log(`Target triple: ${targetTriple}`);

    // Rename the executable
    renameSync(`${source}/Vla${extension}`, `${source}/csharp-${targetTriple}${extension}`);

    const files = readdirSync(source);

    for (const file of files) {
        if (!(file.endsWith(".dll") || file.endsWith(".exe"))) continue;

        let sourceFile = join(source, file);
        let targetFile = join(targetExe, file);

        if (file.endsWith(".dll")) {
            targetFile = join(targetDll, file);
        }

        console.log(`Copying ${sourceFile} to ${targetFile}`);

        copyFileSync(sourceFile, targetFile);
    }
}

main().catch((e) => {
    throw e;
});
