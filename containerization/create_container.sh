#!/bin/bash

set -e

CR_SCRIPT_DIR=$(dirname "$0")
APP_NAME="ogrenci_adiat_sys"
DOCKERFILE_PATH="$CR_SCRIPT_DIR/Dockerfile"
BUILD_DIR="$CR_SCRIPT_DIR/.."
DRY_RUN=0
EXPORT=0
CLEAN_BUILD=0
EXPORT_DIR="$CR_SCRIPT_DIR/exports"

cleanup() {
    $ACTIVE_CONT_RUNTIME_CMD images | grep "$APP_NAME" | awk '{print $3}' | xargs -r $ACTIVE_CONT_RUNTIME_CMD rmi
}

COMMIT_HASH=$(git rev-parse --short HEAD)
TAG=$(git describe --tags --exact-match 2>/dev/null || echo "$COMMIT_HASH")

CONT_RUNTIME_CMD="podman"
ALT_CONT_RUNTIME_CMD="docker"
ACTIVE_CONT_RUNTIME_CMD=$CONT_RUNTIME_CMD

if command -v "$ALT_CONT_RUNTIME_CMD" &>/dev/null; then
    ACTIVE_CONT_RUNTIME_CMD=$ALT_CONT_RUNTIME_CMD
elif ! command -v "$CONT_RUNTIME_CMD" &>/dev/null; then
    echo "Neither Docker nor Podman found. Exiting."
    exit 1
fi

for arg in "$@"; do
    case $arg in
    --dry-run)
        DRY_RUN=1
        ;;
    --export)
        EXPORT=1
        ;;
    --clean-build)
        CLEAN_BUILD=1
        ;;
    *)
        echo "Unknown option: $arg"
        exit 1
        ;;
    esac
done

if [ $DRY_RUN -eq 1 ]; then
    echo "Dry run mode enabled."
    echo "Would remove existing image: $APP_NAME:$COMMIT_HASH"
    echo "Would build new image: $APP_NAME:$COMMIT_HASH from $DOCKERFILE_PATH"
    echo "Would tag image: $APP_NAME:$TAG"
    if [ $EXPORT -eq 1 ]; then
        echo "Would export image to $EXPORT_DIR/$APP_NAME-$COMMIT_HASH.tar.gz"
    fi
    if [ $CLEAN_BUILD -eq 1 ]; then
        echo "Would clean up old images with name: $APP_NAME"
    fi
else
    if [ $CLEAN_BUILD -eq 1 ]; then
        echo "Cleaning up old images with name: $APP_NAME"
        cleanup
    fi

    if $ACTIVE_CONT_RUNTIME_CMD image inspect "$APP_NAME:$COMMIT_HASH" >/dev/null 2>&1; then
        echo "Removing existing image: $APP_NAME:$COMMIT_HASH"
        $ACTIVE_CONT_RUNTIME_CMD rmi "$APP_NAME:$COMMIT_HASH"
    fi

    echo "Building new image: $APP_NAME:$COMMIT_HASH"
    if $ACTIVE_CONT_RUNTIME_CMD build -t "$APP_NAME:$COMMIT_HASH" -f "$DOCKERFILE_PATH" "$BUILD_DIR"; then
        echo "Tagging image: $APP_NAME:$TAG"
        $ACTIVE_CONT_RUNTIME_CMD tag "$APP_NAME:$COMMIT_HASH" "$APP_NAME:$TAG"
    fi

    if [ $EXPORT -eq 1 ]; then
        echo "Exporting image to $EXPORT_DIR/$APP_NAME-$COMMIT_HASH.tar.gz"
        mkdir -p "$EXPORT_DIR"
        $ACTIVE_CONT_RUNTIME_CMD save "$APP_NAME:$COMMIT_HASH" | gzip >"$EXPORT_DIR/$APP_NAME-$COMMIT_HASH.tar.gz"
    fi

    echo "Running cleanup."
    cleanup
fi
