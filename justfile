set windows-shell := ["powershell.exe", "-NoLogo", "-Command"]

default:
    just dev

dev:
    cd app && bun install
    cd app && bun run tauri dev

build:
    cd app && bun install
    cd app && bun run tauri build

schema: schema-gen sync-schema

schema-gen:
    cargo run -p schema --quiet -- --rust-only
    cargo test -p vla export_bindings --quiet
    cargo run -p schema --quiet
    cargo fmt -p vla

sync-schema:
    rm -rf app/frontend/lib/schema
    mkdir -p app/frontend/lib/schema
    cp app/backend/client/*.ts app/frontend/lib/schema/

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
    rm -rf app/backend/bindings
    find app/backend/client -maxdepth 1 -name '*.ts' ! -name 'rpc.ts' -delete
    rm -rf app/frontend/lib/schema
    rm -rf target/bootstrap
