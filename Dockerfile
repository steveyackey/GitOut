# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution and project files
COPY GitOut.sln .
COPY src/GitOut.Domain/GitOut.Domain.csproj src/GitOut.Domain/
COPY src/GitOut.Application/GitOut.Application.csproj src/GitOut.Application/
COPY src/GitOut.Infrastructure/GitOut.Infrastructure.csproj src/GitOut.Infrastructure/
COPY src/GitOut.Console/GitOut.Console.csproj src/GitOut.Console/

# Restore dependencies
RUN dotnet restore GitOut.sln

# Copy source code and build
COPY . .
RUN dotnet publish src/GitOut.Console/GitOut.Console.csproj \
    -c Release \
    -o /app \
    --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/runtime:10.0
WORKDIR /app

# Install git (required for the game)
RUN apt-get update && \
    apt-get install -y --no-install-recommends git && \
    rm -rf /var/lib/apt/lists/*

# Configure git for the container
RUN git config --global user.email "player@gitout.game" && \
    git config --global user.name "Player"

# Copy built application
COPY --from=build /app .

# Set entry point
ENTRYPOINT ["dotnet", "GitOut.Console.dll"]
