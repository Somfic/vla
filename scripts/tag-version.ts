import { readFileSync, writeFileSync } from "fs";
import { glob } from "glob";
import child_process from "child_process";

let bumpType: "major" | "minor" | "patch" | "prerelease" = "minor";

const branch = child_process.execSync("git branch --show-current", { encoding: "utf8" }).trim();
const isPreRelease = branch === "next";
const tags = child_process
    .execSync("git ls-remote --tags --sort=-committerdate", { encoding: "utf8" })
    .trim()
    .split("\n")
    .map((tag) => tag.split("/")[2])
    .map((tag) => tag.replace(/^v/, ""));
const author = child_process.execSync("git log -1", { encoding: "utf8" }).trim().split("\n")[1].replace("Author: ", "");
const isCi = process.env.CI;
const isMergeCommit = author.toLocaleLowerCase().includes("merge");
const commitMessage = child_process.execSync("git log -1 --pretty=%B", { encoding: "utf8" }).trim();
const currentVersion = tags.filter((tag) => !tag.includes("-next"))[0]?.replace(/^v/, "") ?? "0.0.0";
const nextVersion = tags.filter((tag) => tag.includes("-next"))[0]?.split("-next")[0] + "-next" ?? "v0.1.0-next";

// Determine bump type
if (isPreRelease) {
    bumpType = "prerelease";
} else if (isMergeCommit) {
    bumpType = "minor";
} else {
    bumpType = "patch";
}

console.log(`Bump type: ${bumpType}`);
console.log(`Current version: ${currentVersion}`);

const newVersion = determineNewVersion();

// Make sure the version is valid (semver)
const versionRegex = /(\d+\.\d+)(.\d+)?(\-next\.[0-9]{3})?/;
if (!versionRegex.test(newVersion)) {
    console.error(`Invalid version format specified (semver expected, was ${newVersion})`);
    process.exit(1);
}

console.log(`New version:     ${newVersion}`);

updateProjectVersions(newVersion);

if (process.env.CI) {
    pushNewTag(newVersion);
}

function determineNewVersion() {
    let major = parseInt(currentVersion.split(".")[0]);
    let minor = parseInt(currentVersion.split(".")[1]);
    let patch = parseInt(currentVersion.split(".")[2]);

    if (isPreRelease) {
        const nextNumber =
            tags
                .filter((tag) => tag.startsWith(nextVersion))
                .map((tag) => parseInt(tag.split("-next.")[1]))
                .sort((a, b) => b - a)[0] + 1;

        return `${nextVersion}.${nextNumber.toString().padStart(3, "0")}`;
    }

    switch (bumpType) {
        case "major":
            major++;
            minor = 0;
            patch = 0;
            return `${major}.${minor}.${patch}`;
        case "minor":
            minor++;
            patch = 0;
            return `${major}.${minor}.${patch}`;
        case "patch":
            patch++;
            break;
        case "prerelease":
            patch++;
            return `${major}.${minor}.${patch}`;
    }

    return `${major}.${minor}.${patch}`;
}

function updateProjectVersions(version: string) {
    // Clean up version by removing leading zeros. Example: next.001 -> next.1
    version = version.replace(/\.0+(\d+)/, ".$1");

    console.log("Updating project versions to", version);

    const packageJsons = glob.sync("**/package.json", { ignore: ["node_modules/**"] });
    const cargoTomls = glob.sync("**/Cargo.toml", { ignore: ["node_modules/**", "tauri/**"] });
    const tauriConf = glob.sync("**/tauri.conf.json")[0];

    // Update the package.json files
    for (const packageJson of packageJsons) {
        if (packageJson.includes("node_modules")) continue;

        console.log("Updating", packageJson);
        let packageJsonContent = JSON.parse(readFileSync(packageJson, "utf8"));
        packageJsonContent["version"] = version;
        writeFileSync(packageJson, JSON.stringify(packageJsonContent, null, 4), "utf8");
    }

    // Update the Cargo.toml files
    for (const cargoToml of cargoTomls) {
        console.log("Updating", cargoToml);
        let cargoTomlContent = readFileSync(cargoToml, "utf8");
        cargoTomlContent = cargoTomlContent.replace(/version = ".*"/, `version = "${version}"`);
        writeFileSync(cargoToml, cargoTomlContent, "utf8");
    }

    // Update the tauri.conf.json file
    console.log("Updating", tauriConf);
    let tauriConfContent = JSON.parse(readFileSync(tauriConf, "utf8"));
    tauriConfContent["package"]["version"] = version;
    writeFileSync(tauriConf, JSON.stringify(tauriConfContent, null, 4), "utf8");
}

function pushNewTag(version: string) {
    const tag = `${version}`;

    console.log("Pushing new tag", tag, "to remote");

    // Set git identity
    child_process.execSync(`git config user.email "41898282+github-actions[bot]@users.noreply.github.com"`, { encoding: "utf8" });
    child_process.execSync(`git config user.name "github-actions[bot]"`, { encoding: "utf8" });

    // Get latest commit author
    let author = child_process.execSync("git log -1", { encoding: "utf8" }).trim().split("\n")[1].replace("Author: ", "");

    // Stage changes
    child_process.execSync("git add .", { encoding: "utf8" });

    // Commit, tag and push
    let author_cmd = "";
    // Check if author is in valid format (Name <email>)
    if (/^.* <.*>$/.test(author)) {
        author_cmd = `--author="${author}"`;
    }

    // Check if there are any changes
    const status = child_process.execSync("git status --porcelain", { encoding: "utf8" });

    if (status.trim() !== "") {
        child_process.execSync(`git commit -m "🤖 bump to ${"`"}${tag}${"`"}" ${author_cmd}`, { encoding: "utf8" });
    }

    child_process.execSync(`git tag ${tag}`, { encoding: "utf8" });
    child_process.execSync("git push", { encoding: "utf8" });
    child_process.execSync(`git push origin ${tag}`, { encoding: "utf8" });
}
