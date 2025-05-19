# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy and restore
COPY ["VinApplication/VinApplication.csproj", "VinApplication/"]
RUN dotnet restore "VinApplication/VinApplication.csproj"

COPY . .
WORKDIR "/src/VinApplication"

RUN dotnet build "VinApplication.csproj" -c $BUILD_CONFIGURATION -o /app/build
RUN dotnet publish "VinApplication.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final

WORKDIR /app
COPY --from=build /app/publish .

# Run the app
ENTRYPOINT ["dotnet", "VinApplication.dll"]
