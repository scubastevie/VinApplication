FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy project files
COPY VinDecoderWorker/*.csproj ./VinDecoderWorker/
COPY VinData/*.csproj ./VinData/
RUN dotnet restore ./VinDecoderWorker/VinDecoderWorker.csproj

# Copy all source code
COPY . .

# Build and publish
WORKDIR /app/VinDecoderWorker
RUN dotnet publish -c Release -o /out

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /out .

ENTRYPOINT ["dotnet", "VinDecoderWorker.dll"]
