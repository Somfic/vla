{
  description = "vla";
  inputs = {
    nixpkgs.url = "github:NixOS/nixpkgs/nixos-unstable";
    rust-overlay = {
      url = "github:oxalica/rust-overlay";
      inputs.nixpkgs.follows = "nixpkgs";
    };
    flake-utils.url = "github:numtide/flake-utils";
  };

  outputs = { self, nixpkgs, rust-overlay, flake-utils }:
    flake-utils.lib.eachDefaultSystem (system:
      let
        overlays = [ (import rust-overlay) ];
        pkgs = import nixpkgs { inherit system overlays; };

        rustToolchain = pkgs.rust-bin.stable."1.89.0".default.override {
          extensions =
            [ "rust-src" "rust-analyzer" "clippy" "llvm-tools-preview" ];
        };

        libraries = with pkgs; [
          webkitgtk_4_1
          gtk3
          cairo
          gdk-pixbuf
          glib
          dbus
          openssl
          librsvg
        ];
      in {
        devShells.default = pkgs.mkShell {
          nativeBuildInputs = with pkgs; [
            pkg-config
            gobject-introspection
          ];

          buildInputs = with pkgs;
            [
              rustToolchain
              cargo-llvm-cov
              cargo-nextest
              just
              cargo-edit
              cargo-watch
              cargo-deny
              llvm
              ripgrep
              fd
              bat
              bun
            ] ++ pkgs.lib.optionals pkgs.stdenv.isLinux [
              mold
              # Tauri dependencies from nixos wiki
              at-spi2-atk
              atkmm
              cairo
              gdk-pixbuf
              glib
              gtk3
              harfbuzz
              librsvg
              libsoup_3
              pango
              webkitgtk_4_1
              openssl
              dbus
            ];

          shellHook = ''
            export LD_LIBRARY_PATH="${pkgs.lib.makeLibraryPath libraries}:$LD_LIBRARY_PATH"
            export XDG_DATA_DIRS="${pkgs.gsettings-desktop-schemas}/share/gsettings-schemas/${pkgs.gsettings-desktop-schemas.name}:${pkgs.gtk3}/share/gsettings-schemas/${pkgs.gtk3.name}:$XDG_DATA_DIRS"
            export WEBKIT_DISABLE_COMPOSITING_MODE=1
          '';

          RUST_BACKTRACE = "1";
          RUST_LOG = "debug";
        };

        formatter = pkgs.nixpkgs-fmt;
      });
}
