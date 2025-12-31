# GitOut Dockerfile - Multi-stage build for Native AOT
# Produces a minimal runtime image with git included

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Update CA certificates to fix SSL issues
RUN apt-get update && apt-get install -y ca-certificates && update-ca-certificates && rm -rf /var/lib/apt/lists/*

# Copy project files
COPY src/GitOut.Domain/GitOut.Domain.csproj ./src/GitOut.Domain/
COPY src/GitOut.Application/GitOut.Application.csproj ./src/GitOut.Application/
COPY src/GitOut.Infrastructure/GitOut.Infrastructure.csproj ./src/GitOut.Infrastructure/
COPY src/GitOut.Console/GitOut.Console.csproj ./src/GitOut.Console/

# Restore dependencies for the console project (includes all dependencies)
RUN dotnet restore src/GitOut.Console/GitOut.Console.csproj

# Copy source code
COPY src/ ./src/

# Build and publish Native AOT for linux-x64
RUN dotnet publish src/GitOut.Console/GitOut.Console.csproj \
    -c Release \
    -r linux-x64 \
    --self-contained true \
    -p:PublishAot=true \
    -p:StripSymbols=true \
    -o /app/publish

# Runtime stage - minimal base with git
FROM mcr.microsoft.com/dotnet/runtime-deps:10.0-noble AS runtime
WORKDIR /app

# Install git
RUN apt-get update \
    && apt-get install -y --no-install-recommends \
        git \
        ca-certificates \
    && rm -rf /var/lib/apt/lists/*

# Copy the published application
COPY --from=build /app/publish/gitout .

# Create working directory for git operations
RUN mkdir -p /work

# Set environment variables for XDG Base Directory Specification
ENV XDG_DATA_HOME=/app/data \
    XDG_CONFIG_HOME=/app/config

# Create data directories
RUN mkdir -p /app/data /app/config

# Make the binary executable
RUN chmod +x /app/gitout

# Set working directory to /work for git-based challenges
WORKDIR /work

# Run GitOut
ENTRYPOINT ["/app/gitout"]
