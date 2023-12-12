import { execSync } from "child_process";
import { platform } from "os";

switch (platform()) {
    case "linux":
        execSync("sudo apt update");
        execSync(
            "sudo apt install libwebkit2gtk-4.0-dev \
			build-essential \
			curl \
			wget \
			file \
			libssl-dev \
			libgtk-3-dev \
			libayatana-appindicator3-dev \
			librsvg2-dev"
        );
        break;
    case "darwin":
        execSync("xcode-select --install");
        break;
    default:
        console.error(`Unsupported platform: ${platform}`);
    //process.exit(1);
}
