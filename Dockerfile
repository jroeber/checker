# Build
FROM mcr.microsoft.com/dotnet/core/sdk:3.1-alpine AS builder

ARG bin_version="0.0.0"

COPY . .

RUN dotnet publish \
  -c Release \
  -r linux-musl-x64 \
  -p:Version=${bin_version} \
  -p:PublishSingleFile=true \
  -p:PublishTrimmed=true \
  -o build

# Run
FROM mcr.microsoft.com/dotnet/core/runtime-deps:3.1-alpine

COPY --from=builder /build/Checker ./checker

EXPOSE 8080
ENTRYPOINT [ "./checker", "--no-console", "-c", "config.yml" ]