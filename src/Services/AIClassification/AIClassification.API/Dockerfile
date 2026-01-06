FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/Services/AIClassification/AIClassification.API/AIClassification.API.csproj", "src/Services/AIClassification/AIClassification.API/"]
COPY ["src/Services/AIClassification/AIClassification.Application/AIClassification.Application.csproj", "src/Services/AIClassification/AIClassification.Application/"]
COPY ["src/Services/AIClassification/AIClassification.Domain/AIClassification.Domain.csproj", "src/Services/AIClassification/AIClassification.Domain/"]
COPY ["src/Services/AIClassification/AIClassification.Infrastructure/AIClassification.Infrastructure.csproj", "src/Services/AIClassification/AIClassification.Infrastructure/"]
COPY ["src/Shared/Shared.Application/Shared.Application.csproj", "src/Shared/Shared.Application/"]
COPY ["src/Shared/Shared.Domain/Shared.Domain.csproj", "src/Shared/Shared.Domain/"]
COPY ["src/Shared/Shared.Infrastructure/Shared.Infrastructure.csproj", "src/Shared/Shared.Infrastructure/"]

RUN dotnet restore "src/Services/AIClassification/AIClassification.API/AIClassification.API.csproj"
COPY . .
WORKDIR "/src/src/Services/AIClassification/AIClassification.API"
RUN dotnet build "AIClassification.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "AIClassification.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AIClassification.API.dll"]
