# Use PowerShell on Windows
set windows-shell := ["powershell.exe", "-NoLogo", "-Command"]

# Default recipe to display help
default:
    @just --list

# Run development server
dev:
    bun run dev

# Build the project
build:
    bun run build

# Run svelte-check
check:
    bun run check

# Run tests for Rust backend
test:
    cargo test --manifest-path core/Cargo.toml

# Run all checks (svelte-check + cargo check + cargo test)
check-all: check
    cargo check --manifest-path core/Cargo.toml
    cargo test --manifest-path core/Cargo.toml

# Build Rust backend
build-rust:
    cargo build --manifest-path core/Cargo.toml

# Build Rust backend in release mode
build-rust-release:
    cargo build --release --manifest-path core/Cargo.toml

# Run cargo clippy for linting
clippy:
    cargo clippy --manifest-path core/Cargo.toml

# Format Rust code
fmt:
    cargo fmt --manifest-path core/Cargo.toml

# Format and check formatting
fmt-check:
    cargo fmt --check --manifest-path core/Cargo.toml

# Clean build artifacts
clean:
    cargo clean --manifest-path core/Cargo.toml
    bun run build --clean || true

# Install dependencies
install:
    bun install
