import { copyFileSync, existsSync, mkdirSync, readdirSync } from "fs";
import { join } from "path";

const path = join("..", "src-tauri", "target", "release", "bundle", "msi");

// Check the files in the directory
const files = readdirSync(path, { withFileTypes: true })
    .filter((dirent) => dirent.isFile())
    .map((file) => file.name);

if (files.length !== 1) {
    console.error("Expected one file in the directory");
    process.exit(1);
}

// Create the release-files directory if it doesn't exist
if (!existsSync("release-files")) {
    mkdirSync("release-files");
}

const oldFile = join(path, files[0]);
const newFile = join("..", "release-files", files[0]);

console.log(`Copying ${oldFile} to ${newFile}`);
copyFileSync(oldFile, newFile);
