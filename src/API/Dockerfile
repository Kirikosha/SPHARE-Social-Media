# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy project files (note the SocialMediaApp/ prefix)
COPY ["SocialMediaApp/src/API/API.csproj", "API/"]
COPY ["SocialMediaApp/src/Application/Application.csproj", "Application/"]
COPY ["SocialMediaApp/src/Domain/Domain.csproj", "Domain/"]
COPY ["SocialMediaApp/src/Infrastructure/Infrastructure.csproj", "Infrastructure/"]

RUN dotnet restore "API/API.csproj"

# Copy the rest of the source code
COPY SocialMediaApp/src/ .

WORKDIR "/src/API"
RUN dotnet build "./API.csproj" -c $BUILD_CONFIGURATION -o /app/build
RUN dotnet publish "./API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "API.dll"]