# Build stage - use SDK to compile Native AOT binary
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG TARGETARCH
WORKDIR /src

# Install clang for Native AOT compilation
RUN apt-get update && \
    apt-get install -y --no-install-recommends clang zlib1g-dev && \
    rm -rf /var/lib/apt/lists/*

# Map Docker arch to .NET RID
RUN echo "TARGETARCH=$TARGETARCH" && \
    if [ "$TARGETARCH" = "arm64" ]; then \
        echo "linux-arm64" > /tmp/rid; \
    else \
        echo "linux-x64" > /tmp/rid; \
    fi

# Copy project files
COPY src/GitOut.Domain/GitOut.Domain.csproj src/GitOut.Domain/
COPY src/GitOut.Application/GitOut.Application.csproj src/GitOut.Application/
COPY src/GitOut.Infrastructure/GitOut.Infrastructure.csproj src/GitOut.Infrastructure/
COPY src/GitOut.Console/GitOut.Console.csproj src/GitOut.Console/

# Restore dependencies for target architecture
RUN dotnet restore src/GitOut.Console/GitOut.Console.csproj -r $(cat /tmp/rid)

# Copy source code and build Native AOT
COPY . .
RUN dotnet publish src/GitOut.Console/GitOut.Console.csproj \
    -c Release \
    -r $(cat /tmp/rid) \
    --self-contained \
    -o /app

# Runtime stage - minimal image with just git
FROM debian:bookworm-slim
WORKDIR /app

# Install git and libicu (required for the game and .NET globalization)
RUN apt-get update && \
    apt-get install -y --no-install-recommends git ca-certificates libicu72 && \
    rm -rf /var/lib/apt/lists/*

# Configure git for the container
RUN git config --global user.email "player@gitout.game" && \
    git config --global user.name "Player"

# Copy the Native AOT binary (single file, no .NET runtime needed)
COPY --from=build /app/gitout .

# Set entry point - run the native binary directly
ENTRYPOINT ["./gitout"]
