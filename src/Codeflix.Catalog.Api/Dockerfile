FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/Codeflix.Catalog.Api/Codeflix.Catalog.Api.csproj", "src/Codeflix.Catalog.Api/"]
RUN dotnet restore "src/Codeflix.Catalog.Api/Codeflix.Catalog.Api.csproj"
COPY . .
WORKDIR "/src/src/Codeflix.Catalog.Api"
RUN dotnet build "./Codeflix.Catalog.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Codeflix.Catalog.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Codeflix.Catalog.Api.dll"]
