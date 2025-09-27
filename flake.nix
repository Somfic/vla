{
  description = "VLA - A Tauri App with SvelteKit frontend";

  inputs = {
    nixpkgs.url = "github:NixOS/nixpkgs/nixos-unstable";
    rust-overlay.url = "github:oxalica/rust-overlay";
    flake-utils.url = "github:numtide/flake-utils";
  };

  outputs = { self, nixpkgs, rust-overlay, flake-utils }:
    flake-utils.lib.eachDefaultSystem (system:
      let
        overlays = [ (import rust-overlay) ];
        pkgs = import nixpkgs {
          inherit system overlays;
        };

        rustToolchain = pkgs.rust-bin.stable.latest.default.override {
          extensions = [ "rust-src" "rustfmt" "clippy" ];
        };

        libraries = with pkgs; [
          webkitgtk_4_1
          gtk3
          cairo
          gdk-pixbuf
          glib
          dbus
          openssl_3
          librsvg
          libsoup_3
          pango
          atk
          at-spi2-atk
          harfbuzz
        ];

        packages = with pkgs; [
          rustToolchain
          cargo-tauri
          bun
          pkg-config
          gobject-introspection
          git
          curl
          wget
          dbus
          openssl_3
          glib
          gtk3
          libsoup_3
          webkitgtk_4_1
          librsvg
          pango
          atk
          at-spi2-atk
          harfbuzz
        ];

      in
      {
        devShells.default = pkgs.mkShell {
          buildInputs = packages;
          nativeBuildInputs = [ pkgs.pkg-config ];

          shellHook = ''
            export RUST_SRC_PATH="${rustToolchain}/lib/rustlib/src/rust/library"
            export LD_LIBRARY_PATH=${pkgs.lib.makeLibraryPath libraries}:$LD_LIBRARY_PATH
            export XDG_DATA_DIRS=${pkgs.gsettings-desktop-schemas}/share/gsettings-schemas/${pkgs.gsettings-desktop-schemas.name}:${pkgs.gtk3}/share/gsettings-schemas/${pkgs.gtk3.name}:$XDG_DATA_DIRS
            export WEBKIT_DISABLE_COMPOSITING_MODE=1
            export RUST_BACKTRACE=1
            
            # Install frontend dependencies if they don't exist
            if [ ! -d "node_modules" ]; then
              echo "Installing frontend dependencies..."
              bun install
            fi
          '';

          # Environment variables for Tauri
          WEBKIT_DISABLE_COMPOSITING_MODE = "1";
          RUST_BACKTRACE = "1";
        };

        # Package for building the application
        packages.default = pkgs.rustPlatform.buildRustPackage rec {
          pname = "vla";
          version = "0.1.0";

          src = ./.;

          cargoLock = {
            lockFile = ./core/Cargo.lock;
          };

          nativeBuildInputs = packages;
          buildInputs = libraries;

          meta = with pkgs.lib; {
            description = "VLA - A Tauri App";
            homepage = "https://github.com/your-username/vla";
            license = licenses.mit;
            maintainers = [ ];
            platforms = platforms.linux ++ platforms.darwin;
          };
        };

        # Formatter for the flake
        formatter = pkgs.nixpkgs-fmt;
      });
}