import child_process from "child_process";

child_process.execSync("cd src && cd extensions && cd test && yarn && yarn run build");
