#!/bin/bash
set -euo pipefail

# GitOut Native AOT Publishing Script (Linux/macOS)
# Builds self-contained Native AOT binaries for all target platforms

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"
PROJECT_PATH="$REPO_ROOT/src/GitOut.Console/GitOut.Console.csproj"
ARTIFACTS_DIR="$REPO_ROOT/artifacts/publish"

echo "==> GitOut Native AOT Publishing Script"
echo "==> Repository: $REPO_ROOT"
echo "==> Artifacts: $ARTIFACTS_DIR"
echo ""

# Target Runtime Identifiers
RIDS=(
    "linux-x64"
    "linux-arm64"
    "osx-x64"
    "osx-arm64"
)

# Optional musl builds for Alpine
# Uncomment to include:
# RIDS+=("linux-musl-x64" "linux-musl-arm64")

# Clean artifacts directory
echo "==> Cleaning artifacts directory..."
rm -rf "$ARTIFACTS_DIR"
mkdir -p "$ARTIFACTS_DIR"

# Build for each RID
for rid in "${RIDS[@]}"; do
    echo ""
    echo "==> Publishing for $rid..."
    
    OUTPUT_DIR="$ARTIFACTS_DIR/$rid"
    
    dotnet publish "$PROJECT_PATH" \
        -c Release \
        -r "$rid" \
        --self-contained true \
        -p:PublishAot=true \
        -p:StripSymbols=true \
        -o "$OUTPUT_DIR"
    
    if [ $? -eq 0 ]; then
        echo "✓ Published $rid successfully"
        
        # Show output size
        if [ -f "$OUTPUT_DIR/gitout" ]; then
            SIZE=$(du -h "$OUTPUT_DIR/gitout" | cut -f1)
            echo "  Binary size: $SIZE"
        fi
    else
        echo "✗ Failed to publish $rid"
        exit 1
    fi
done

echo ""
echo "==> Publishing complete!"
echo "==> Artifacts location: $ARTIFACTS_DIR"
echo ""
echo "Available binaries:"
find "$ARTIFACTS_DIR" -name "gitout" -o -name "gitout.exe" | sort
