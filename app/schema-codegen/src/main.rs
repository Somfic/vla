use std::path::Path;

/// CLI entry. The justfile runs this from the repo root, so `.` is the root.
fn main() {
    let rust_only = std::env::args().skip(1).any(|a| a == "--rust-only");
    schema_codegen::generate(Path::new("."), rust_only);
}
