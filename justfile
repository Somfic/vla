set windows-shell := ["powershell.exe", "-NoLogo", "-Command"]

default:
    just dev

dev:
    cd desktop && bun install
    cd desktop && bun run tauri dev

dev-web:
    cd web && bun install
    cd web && bun run dev

build:
    cd desktop && bun install
    cd desktop && bun run tauri build

build-web:
    cd web && bun install
    cd web && bun run build

schema: schema-gen sync-schema

schema-gen:
    cargo run -p schema-codegen --bin codegen --quiet -- --rust-only
    cargo test -p schema export_bindings --quiet
    cargo run -p schema-codegen --bin codegen --quiet

sync-schema:
    rm -rf web/src/lib/schema desktop/src/lib/schema
    mkdir -p web/src/lib/schema desktop/src/lib/schema
    cp schema/client/*.ts web/src/lib/schema/
    cp schema/client/*.ts desktop/src/lib/schema/

check: check-rust check-desktop check-web

check-rust: schema
    cargo fmt --all -- --check
    cargo clippy --workspace --all-targets -- -D warnings
    cargo test --workspace

check-desktop: schema
    cd desktop && bun install
    cd desktop && bun run check

check-web: schema
    cd web && bun install
    cd web && bun run check

fmt:
    cargo fmt --all

clean:
    rm -f schema/src/_generated.rs
    rm -rf schema/bindings
    find schema/client -maxdepth 1 -name '*.ts' ! -name 'rpc.ts' -delete
    rm -rf web/src/lib/schema desktop/src/lib/schema
    rm -rf target/bootstrap
