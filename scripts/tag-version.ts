import { readFileSync, writeFileSync } from "fs";
import { glob } from "glob";
import child_process from "child_process";

// Get command line argument for the version
var version = "";

if (process.argv.length < 3) {
    console.log("No version specified, attempting to determine from commit message");

    var commit = child_process.execSync("git log -1 --pretty=%B", { encoding: "utf8" }).trim().split("\n")[0];

    if (commit.toLocaleLowerCase().includes("merge")) {
        console.log("Merging commit detected");
        version = "minor";
    } else {
        version = "patch";
    }
} else {
    var version = process.argv[2].trim();
}

if (version === "major" || version === "minor" || version === "patch") {
    // Get the current version

    // Run git tag --sort=committerdate
    var latestTag = child_process.execSync("git ls-remote --tags --sort=committerdate", { encoding: "utf8" }).split("\n")[0];
    var currentVersion = latestTag.split("/")[2].replace("v", "");

    console.log("Current version is " + currentVersion);
    console.log("Performing " + version + " increment");

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

const packageJsons = glob.sync("**/package.json", { ignore: ["node_modules/**"] });
const cargoToml = glob.sync("**/Cargo.toml")[0];
const tauriConf = glob.sync("**/tauri.conf.json")[0];

// Update the package.json file
for (const packageJson of packageJsons) {
    console.log(`Updating ${packageJson} to version ${version}`);
    let packageJsonContent = JSON.parse(readFileSync(packageJson, "utf8"));
    packageJsonContent["version"] = version;
    writeFileSync(packageJson, JSON.stringify(packageJsonContent, null, 4), "utf8");
}

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

// Commit and push and tag
child_process.execSync("git add .", { encoding: "utf8" });

// Get latest commit author
var author = child_process.execSync("git log -1", { encoding: "utf8" }).trim().split("\n")[1].replace("Author: ", "");

// Set git identity
child_process.execSync(`git config user.email "41898282+github-actions[bot]@users.noreply.github.com"`, { encoding: "utf8" });
child_process.execSync(`git config user.name "github-actions[bot]"`, { encoding: "utf8" });

// Commit, tag and push
child_process.execSync(`git commit -m "chore: release v${version}" --author="${author}"`, { encoding: "utf8" });
child_process.execSync(`git tag v${version}`, { encoding: "utf8" });
child_process.execSync("git push", { encoding: "utf8" });
child_process.execSync(`git push origin v${version}`, { encoding: "utf8" });
