import { readFileSync, writeFileSync } from "fs";
import { join } from "path";

// Get command line argument for the version
const version = process.argv[2].trim();
if (!version) {
    console.error("No version specified");
    process.exit(1);
}

// Make sure the version is valid (semver)
const versionRegex = /\d+\.\d+\.\d+/;
if (!versionRegex.test(version)) {
    console.error("Invalid version format specified");
    process.exit(1);
}

const packageJson = "package.json";
const cargoToml = join("src-tauri", "Cargo.toml");
const tauriConf = join("src-tauri", "tauri.conf.json");
const csharp = join("src-csharp", "Vla", "Vla.csproj");

// Update the package.json file
console.log(`Updating ${packageJson} to version ${version}`);
let packageJsonContent = JSON.parse(readFileSync(packageJson, "utf8"));
packageJsonContent["version"] = version;
writeFileSync(packageJson, JSON.stringify(packageJsonContent, null, 4), "utf8");

// Update the Cargo.toml file
console.log(`Updating ${cargoToml} to version ${version}`);
let cargoTomlContent = readFileSync(cargoToml, "utf8");
cargoTomlContent = cargoTomlContent.replace(/version = ".*"/, `version = "${version}"`);
writeFileSync(cargoToml, cargoTomlContent, "utf8");

// Update the tauri.conf.json file
console.log(`Updating ${tauriConf} to version ${version}`);
let tauriConfContent = JSON.parse(readFileSync(tauriConf, "utf8"));
tauriConfContent["package"]["version"] = version;
writeFileSync(tauriConf, JSON.stringify(tauriConfContent, null, 4), "utf8");

// Update the Vla.csproj file
console.log(`Updating ${csharp} to version ${version}`);
let csharpContent = readFileSync(csharp, "utf8");
csharpContent = csharpContent.replace(/<Version>.*<\/Version>/, `<Version>${version}</Version>`);
csharpContent = csharpContent.replace(/<AssemblyVersion>.*<\/AssemblyVersion>/, `<AssemblyVersion>${version}</AssemblyVersion>`);
csharpContent = csharpContent.replace(/<FileVersion>.*<\/FileVersion>/, `<FileVersion>${version}</FileVersion>`);
writeFileSync(csharp, csharpContent, "utf8");
