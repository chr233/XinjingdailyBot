# Dockerfile for XinjingdailyBot.WebAPI
# Multi-stage build with configurable .NET version via build-arg
ARG DOTNET_VERSION=10.0

ARG PROJECT_NAME="XinjingdailyBot.WebAPI"
ARG TARGET_FRAMEWORK="net10.0"

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:${DOTNET_VERSION} AS build

ARG PROJECT_NAME
ARG TARGETARCH=amd64
ARG TARGETOS=linux
ARG TARGET_FRAMEWORK
ARG CONFIGURATION=Release
ENV DOTNET_CLI_TELEMETRY_OPTOUT=true
ENV DOTNET_NOLOGO=true

WORKDIR /src

# Copy solution and project files first to leverage Docker layer cache
COPY . .

RUN pwsh -NoProfile  -File ./docker-build.ps1 \
    -TargetOS "$TARGETOS" \
    -TargetArch "$TARGETARCH" \
    -Configuration "$CONFIGURATION" \
    -ProjectName "$PROJECT_NAME" \
    -TargetFramework "$TARGET_FRAMEWORK" 

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:${DOTNET_VERSION} AS runtime

ENV DOTNET_CLI_TELEMETRY_OPTOUT=true
ENV DOTNET_NOLOGO=true

WORKDIR /app

# Copy the published files from the build stage
COPY --from=build /publish/ ./

ENV DOTNET_RUNNING_IN_CONTAINER=true

VOLUME ["/app/config", "/app/logs"]
HEALTHCHECK CMD ["pidof", "-q", "dotnet"]

ENTRYPOINT ["dotnet","./XinjingdailyBot.WebAPI.dll"]