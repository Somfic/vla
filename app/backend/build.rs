use std::path::Path;

fn main() {
    // Regenerate `src/_generated.rs` from the schema in `src/` before this
    // crate compiles, so a schema edit can never leave a stale generated file
    // that breaks the build. `CARGO_MANIFEST_DIR` is `<repo>/app/backend`, so
    // `../..` is the repo root the codegen resolves its paths against.
    let manifest = std::env::var("CARGO_MANIFEST_DIR").expect("CARGO_MANIFEST_DIR");
    let root = Path::new(&manifest).join("../..");
    schema_codegen::generate(&root, true);

    // Rerun when any backend source changes. Codegen only rewrites
    // `_generated.rs` when its content actually differs, so re-touching it
    // here does not cause a rebuild loop.
    println!("cargo:rerun-if-changed=src");

    tauri_build::build();
}
