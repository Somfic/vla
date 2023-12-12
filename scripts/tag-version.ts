import { readFileSync, writeFileSync } from "fs";
import { join } from "path";

// Get command line argument for the version
var version = process.argv[2].trim();

if (version === "") {
    console.error("No version specified");
    process.exit(1);
}

if (version === "major" || version === "minor" || version === "patch") {
    // Get the current version
    var currentVersion = (await (await fetch("https://api.github.com/repos/somfic/vla/releases/latest")).json()).tag_name.replace("v", "");

    console.log("Current version is " + currentVersion + " " + version);

    var major = parseInt(currentVersion.split(".")[0]);
    var minor = parseInt(currentVersion.split(".")[1]);
    var patch = parseInt(currentVersion.split(".")[2]);

    switch (version) {
        case "major":
            major++;
            minor = 0;
            patch = 0;
            break;
        case "minor":
            minor++;
            patch = 0;
            break;
        case "patch":
            patch++;
            break;
    }

    console.log(`Incrementing version to ${major}.${minor}.${patch}`);

    version = `${major}.${minor}.${patch}`;
} else {
    var match = version.match(/v(\d+\.\d+)(.\d+)?/);

    if (!match) {
        console.error(`Invalid version format specified (semver expected, was ${version})`);
        process.exit(1);
    }

    version = match[1];

    if (match[2]) {
        version += match[2];
    } else {
        version += ".0";
    }
}

// Make sure the version is valid (semver)
const versionRegex = /^\d+\.\d+\.\d+$/;
if (!versionRegex.test(version)) {
    console.error(`Invalid version format specified (semver expected, was ${version})`);
    process.exit(1);
}

const packageJson = "package.json";
const cargoToml = join("src-tauri", "Cargo.toml");
const tauriConf = join("src-tauri", "tauri.conf.json");

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
