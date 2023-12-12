#!/bin/bash

os=$(uname)

case "$os" in
  Linux)
    sudo apt update
    sudo apt install -y libwebkit2gtk-4.0-dev \
                        build-essential \
                        curl \
                        wget \
                        file \
                        libssl-dev \
                        libgtk-3-dev \
                        libayatana-appindicator3-dev \
                        librsvg2-dev
    ;;
  Darwin)
    xcode-select --install
    ;;
  *)
    echo "Unsupported platform: $os"
    ;;
esac

# Create dist folder
mkdir -p dist