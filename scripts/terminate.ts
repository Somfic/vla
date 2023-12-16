import { execa } from "execa";

if (process.platform === "win32") {
    execa("taskkill", ["/IM", "csharp.exe", "/F"]);
} else if (process.platform === "darwin") {
    execa("pkill", ["csharp"]);
} else {
    console.error("Unsupported platform");
}
