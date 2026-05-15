set windows-shell := ["powershell.exe", "-NoLogo", "-Command"]

# ts-rs per-type intermediates land here (under target/, never in /backend).
# Read by the schema codegen via the same env var.
export TS_RS_EXPORT_DIR := justfile_directory() / "target" / "schema-bindings"

default:
    just dev

dev:
    cd app && bun install
    cd app && bun run tauri dev

build:
    cd app && bun install
    cd app && bun run tauri build

schema: schema-gen

schema-gen:
    cargo run -p schema --quiet -- --rust-only
    cargo test -p vla export_bindings --quiet
    cargo run -p schema --quiet
    cargo fmt -p vla

check: check-rust check-app

check-rust: schema
    cargo fmt --all -- --check
    cargo clippy --workspace --all-targets -- -D warnings
    cargo test --workspace

check-app: schema
    cd app && bun install
    cd app && bun run check

fmt:
    cargo fmt --all

clean:
    rm -f app/backend/src/_generated.rs
    rm -rf target/schema-bindings target/bootstrap
    find app/frontend/lib/schema -maxdepth 1 -name '*.ts' ! -name 'rpc.ts' -delete
