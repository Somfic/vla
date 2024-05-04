import { readFileSync, writeFileSync } from "fs";
import { glob } from "glob";
import child_process from "child_process";

// Get command line argument for the version
var version = "";
var tag = "";

const branch = child_process.execSync("git branch --show-current", { encoding: "utf8" }).trim();

if (process.argv.length <= 2) {
    console.log("No version specified, attempting to determine from commit message");

    var author = child_process.execSync("git log -1", { encoding: "utf8" }).trim().split("\n")[1].replace("Author: ", "");

    if (branch == "main") {
        if (author.toLocaleLowerCase().includes("merge")) {
            console.log("Merging commit detected");
            version = "minor";
        } else {
            version = "patch";
        }
    } else {
        version = "minor";
    }
} else {
    var version = process.argv[2].trim();
}

var dry = false;
if (process.argv.length >= 3) {
    dry = process.argv[3] === "dry";
}

if (version === "major" || version === "minor" || version === "patch") {
    console.log(`Bumping version by ${version}`);

    // Run git tag --sort=committerdate
    var latestTag = child_process
        .execSync("git ls-remote --tags --sort=-committerdate", { encoding: "utf8" })
        .trim()
        .split("\n")
        .filter((tag) => !tag.includes("next"))[0];

    var currentVersion = "0.0.0";
    if (latestTag.split("/")[2] !== undefined) {
        currentVersion = latestTag.split("/")[2].replace("v", "");
    }

    console.log(`Current version: '${currentVersion}'`);

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

    version = `${major}.${minor}.${patch}`;
    tag = `v${version}`;

    if (branch == "next") {
        tag += "-next";

        // Remove previous next tag
        var alphaTags = child_process
            .execSync("git tag --list", { encoding: "utf8" })
            .trim()
            .split("\n")
            .filter((tag) => tag.startsWith(version));

        // Get the highest tag
        var amountOfTags = alphaTags.map((tag) => parseInt(tag.split("-next.")[1]));
        amountOfTags.sort((a, b) => a - b);

        var highestTag = 0;

        if (amountOfTags.length > 0) {
            highestTag = amountOfTags[amountOfTags.length - 1];
        }

        tag += ".";

        // Format with three digits
        tag += (highestTag + 1).toString().padStart(3, "0");
    }
} else {
    var match = version.match(/(\d+\.\d+)(.\d+)(\-next)?/);

    if (!match) {
        console.error(`Invalid version format specified (semver expected, was ${version})`);
        process.exit(1);
    }
}

// Make sure the version is valid (semver)
const versionRegex = /(\d+\.\d+)(.\d+)?(\-next)?/;
if (!versionRegex.test(version)) {
    console.error(`Invalid version format specified (semver expected, was ${version})`);
    process.exit(1);
}

console.log(`New version: '${version}' (${tag})`);

if (dry) process.exit(0);

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

// Check if running in CI/CD
if (process.env.CI) {
    console.log("CI detected, pushing changes");

    // Set git identity
    child_process.execSync(`git config user.email "41898282+github-actions[bot]@users.noreply.github.com"`, { encoding: "utf8" });
    child_process.execSync(`git config user.name "github-actions[bot]"`, { encoding: "utf8" });

    // Get latest commit author
    var author = child_process.execSync("git log -1", { encoding: "utf8" }).trim().split("\n")[1].replace("Author: ", "");

    // Stage changes
    child_process.execSync("git add .", { encoding: "utf8" });

    // Commit, tag and push
    var author_cmd = "";
    // Check if author is in valid format (Name <email>)
    if (/^.* <.*>$/.test(author)) {
        author_cmd = `--author="${author}"`;
    }

    child_process.execSync(`git commit -m "chore: release v${version}" ${author_cmd}`, { encoding: "utf8" });
    child_process.execSync(`git tag ${tag}`, { encoding: "utf8" });
    child_process.execSync("git push", { encoding: "utf8" });
    child_process.execSync(`git push origin v${version}`, { encoding: "utf8" });
}
