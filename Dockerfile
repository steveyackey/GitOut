# Build stage - use SDK to compile Native AOT binary
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Install clang for Native AOT compilation
RUN apt-get update && \
    apt-get install -y --no-install-recommends clang zlib1g-dev && \
    rm -rf /var/lib/apt/lists/*

# Copy project files
COPY src/GitOut.Domain/GitOut.Domain.csproj src/GitOut.Domain/
COPY src/GitOut.Application/GitOut.Application.csproj src/GitOut.Application/
COPY src/GitOut.Infrastructure/GitOut.Infrastructure.csproj src/GitOut.Infrastructure/
COPY src/GitOut.Console/GitOut.Console.csproj src/GitOut.Console/

# Restore dependencies
RUN dotnet restore src/GitOut.Console/GitOut.Console.csproj -r linux-x64

# Copy source code and build Native AOT
COPY . .
RUN dotnet publish src/GitOut.Console/GitOut.Console.csproj \
    -c Release \
    -r linux-x64 \
    --self-contained \
    -o /app

# Runtime stage - minimal image with just git
FROM debian:bookworm-slim
WORKDIR /app

# Install git (required for the game)
RUN apt-get update && \
    apt-get install -y --no-install-recommends git ca-certificates && \
    rm -rf /var/lib/apt/lists/*

# Configure git for the container
RUN git config --global user.email "player@gitout.game" && \
    git config --global user.name "Player"

# Copy the Native AOT binary (single file, no .NET runtime needed)
COPY --from=build /app/gitout .

# Set entry point - run the native binary directly
ENTRYPOINT ["./gitout"]
