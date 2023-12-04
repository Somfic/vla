import { execa } from "execa";
import { renameSync, mkdirSync, existsSync, copyFileSync, rmdirSync, readdirSync, readFileSync, writeFileSync } from "fs";
import { join } from "path";

const dotnetVersion = "net8.0";
const targetExecutable = join("src-tauri", "binaries");
const targetLibraries = join("src-tauri");

async function main() {
    // Terminate previous C# processes
    if (process.platform === "win32") {
        execa("taskkill", ["/IM", "csharp.exe", "/F"]);
    } else if (process.platform === "darwin") {
        execa("pkill", ["csharp"]);
    }

    // Remove the previous C# executable
    if (existsSync(targetExecutable)) {
        rmdirSync(targetExecutable, { recursive: true });
    }
    mkdirSync(targetExecutable, { recursive: true });

    // Build the C# project
    console.log("Cleaning previous builds");
    await execa("dotnet", ["clean", join("src-csharp", "vla"), "-c", "Release"]);

    console.log("Building C# project");
    await execa("dotnet", ["publish", join("src-csharp", "vla"), "-c", "Release", "-f", dotnetVersion, "--self-contained", "true", "/p:PublishSingleFile=true"]);

    console.log("Copying C# executable to src-tauri/binaries");

    // Determine the architecture
    const dist = join("src-csharp", "Vla", "bin", "Release", dotnetVersion);
    const arch = readdirSync(dist, { withFileTypes: true })
        .filter((dirent) => dirent.isDirectory())
        .map((dirent) => dirent.name)[0];

    const source = join("src-csharp", "Vla", "bin", "Release", dotnetVersion, arch, "publish");

    // Determine target triple
    const rustInfo = (await execa("rustc", ["-vV"])).stdout;
    const targetTripleMatch = /host: (\S+)/g.exec(rustInfo);
    const targetTriple = targetTripleMatch ? targetTripleMatch[1] : null;
    if (!targetTriple) {
        console.error("Failed to determine platform target triple");
    }

    console.log(`Preparing build for ${arch} (${targetTriple})`);

    const extension = arch.startsWith("win") ? ".exe" : "";

    // Rename the main executable
    const oldExecutable = join(source, `Vla${extension}`);
    const newExecutable = join(source, `csharp-${targetTriple}${extension}`);
    console.log(`Renaming ${oldExecutable} to ${newExecutable}`);
    renameSync(oldExecutable, newExecutable);

    // Copy the main executable to the binaries folder
    const targetExecutableFile = join(targetExecutable, `csharp-${targetTriple}${extension}`);
    console.log(`Copying ${newExecutable} to ${targetExecutableFile}`);
    copyFileSync(newExecutable, targetExecutableFile);

    // Copy the libraries to the libraries folder
    const libraryFiles = readdirSync(source, { withFileTypes: true })
        .filter((dirent) => dirent.isFile())
        .filter((file) => file.name.endsWith(".dll") || file.name.endsWith(".dylib") || file.name.endsWith(".so"))
        .map((file) => file.name);

    for (const libraryFile of libraryFiles) {
        const sourceFile = join(source, libraryFile);
        const targetFile = join(targetLibraries, libraryFile);
        console.log(`Copying ${sourceFile} to ${targetFile}`);
        copyFileSync(sourceFile, targetFile);
    }

    // Copy the libraries from 'runtimes/{arch}' to the libraries/runtimes/{arch} folder
    const sourceRuntime = join(source, "runtimes", arch);
    const targetRuntime = join(targetLibraries, "runtimes", arch);

    if (!existsSync(targetRuntime)) {
        mkdirSync(targetRuntime, { recursive: true });
    }

    const runtimeFiles = readdirSync(sourceRuntime, { withFileTypes: true })
        .filter((dirent) => dirent.isFile())
        .filter((file) => file.name.endsWith(".dll") || file.name.endsWith(".dylib") || file.name.endsWith(".so"))
        .map((file) => file.name);

    for (const runtimeFile of runtimeFiles) {
        const sourceFile = join(sourceRuntime, runtimeFile);
        const targetFile = join(targetRuntime, runtimeFile);
        console.log(`Copying ${sourceFile} to ${targetFile}`);
        copyFileSync(sourceFile, targetFile);
        libraryFiles.push(join("runtimes", arch, runtimeFile));
    }

    console.log(`Modifying tauri.conf.json to include: ${libraryFiles}`);
    // Read the tauri.conf.json file in src-tauri/tauri.conf.json
    const tauriConf = JSON.parse(readFileSync("src-tauri/tauri.conf.json", "utf8"));

    tauriConf["tauri"]["bundle"]["resources"] = libraryFiles;

    // Write the tauri.conf.json file in src-tauri/tauri.conf.json
    writeFileSync("src-tauri/tauri.conf.json", JSON.stringify(tauriConf, null, 4), "utf8");

    console.log(JSON.stringify(tauriConf, null, 4));
}

main().catch((e) => {
    throw e;
});
