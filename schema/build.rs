use std::process::Command;

fn main() {
    println!("cargo:rerun-if-changed=src/_generated.rs");
    if let Ok(entries) = std::fs::read_dir("src") {
        for entry in entries.flatten() {
            let path = entry.path();
            if path.is_file() && path.extension().and_then(|e| e.to_str()) == Some("rs") {
                println!("cargo:rerun-if-changed={}", path.display());
            }
        }
    }

    // Recursion guard. `just schema` invokes `cargo test -p schema`, which
    // re-runs this build script — break the loop on the recursive call.
    if std::env::var_os("VLA_BOOTSTRAP").is_some() {
        return;
    }

    let status = Command::new("just")
        .env("VLA_BOOTSTRAP", "1")
        .env("CARGO_TARGET_DIR", "target/bootstrap")
        .current_dir("..")
        .arg("schema")
        .status();

    match status {
        Ok(s) if s.success() => {}
        Ok(s) => panic!("`just schema` failed: {s}"),
        Err(err) => panic!("couldn't run `just schema` ({err}). Install just or run manually."),
    }
}
