# Use PowerShell on Windows
set windows-shell := ["powershell.exe", "-NoLogo", "-Command"]

default:
    just dev

dev:
    cd frontend && bun install && cd .. && frontend/node_modules/.bin/tauri dev

build:
    cd frontend && bun install && cd .. && frontend/node_modules/.bin/tauri build

check:
    cd frontend && bun install
    cd frontend && bun run check
    cargo fmt --all --manifest-path core/Cargo.toml -- --check
    cargo clippy --all --manifest-path core/Cargo.toml
    cargo test --all --manifest-path core/Cargo.toml
