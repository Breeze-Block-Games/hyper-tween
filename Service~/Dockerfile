# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
# If APP_UID isn't set at runtime, this will fail. You can default it if needed:
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Build image
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Native AOT toolchain (only needed if you publish AOT; harmless otherwise)
RUN apt-get update && apt-get install -y --no-install-recommends \
    clang gcc libc6-dev \
 && rm -rf /var/lib/apt/lists/*

# Restore with a minimal graph first (faster, better cache hit)
COPY ["Service~/Service.csproj", "Service~/"]
COPY ["Shared~/Shared.csproj", "Shared~/"]
COPY ["Shared/", "Shared/"]
RUN dotnet restore "Service~/Service.csproj"

# Now bring the rest of the sources
COPY . .

# Build
WORKDIR "/src/Service~"
RUN dotnet build "./Service.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish with portable PDBs emitted next to DLLs
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Service.csproj" -c $BUILD_CONFIGURATION -o /app/publish \
    -p:DebugSymbols=true \
    -p:DebugType=portable \
    -p:UseAppHost=false

# Final image (includes DLLs + PDBs)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish ./
ENTRYPOINT ["dotnet", "BreezeBlockGames.PackageBase.Service.dll"]
