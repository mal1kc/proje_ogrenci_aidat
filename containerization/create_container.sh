#!/bin/bash
set -e

CR_SCRIPT_DIR=$(dirname "$0")

APP_NAME="OgrenciAdiatSistemi"
DOCKERFILE_PATH="$CR_SCRIPT_DIR/Dockerfile"
DRY_RUN=0
EXPORT=0
EXPORT_DIR="$CR_SCRIPT_DIR/exported_images"

cleanup() {
    $ACTIVE_CONT_RUNTIME_CMD images | grep "$APP_NAME" | awk '{print $3}' | xargs -r $ACTIVE_CONT_RUNTIME_CMD rmi
}

COMMIT_HASH=$(git rev-parse --short HEAD)
TAG=$(git describe --tags --exact-match 2>/dev/null)

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
        shift
        ;;
    --export)
        EXPORT=1
        shift
        ;;
    esac
done

if [ $DRY_RUN -eq 0 ]; then
    if $ACTIVE_CONT_RUNTIME_CMD image inspect "$APP_NAME:$COMMIT_HASH" >/dev/null 2>&1; then
        $ACTIVE_CONT_RUNTIME_CMD rmi "$APP_NAME:$COMMIT_HASH"
    fi

    if $ACTIVE_CONT_RUNTIME_CMD build -t "$APP_NAME:$COMMIT_HASH" -f "$DOCKERFILE_PATH" .; then
        $ACTIVE_CONT_RUNTIME_CMD tag "$APP_NAME:$COMMIT_HASH" "$APP_NAME:$TAG"
    fi

    if [ $EXPORT -eq 1 ]; then
        mkdir -p "$EXPORT_DIR"
        $ACTIVE_CONT_RUNTIME_CMD save "$APP_NAME:$COMMIT_HASH" | gzip >"$EXPORT_DIR/$APP_NAME-$COMMIT_HASH.tar.gz"
    fi

    cleanup
else
    echo "Dry run. No actions were performed."
fi
