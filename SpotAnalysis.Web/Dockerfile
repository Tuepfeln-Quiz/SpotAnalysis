FROM mcr.microsoft.com/dotnet/aspnet:10.0-noble-chiseled AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["SpotAnalysis.Web/SpotAnalysis.Web.csproj", "SpotAnalysis.Web/"]
COPY ["SpotAnalysis.Services/SpotAnalysis.Services.csproj", "SpotAnalysis.Services/"]
COPY ["SpotAnalysis.Data/SpotAnalysis.Data.csproj", "SpotAnalysis.Data/"]
COPY ["ExcelImportExport/ExcelImportExport.csproj", "ExcelImportExport/"]
RUN dotnet restore "SpotAnalysis.Web/SpotAnalysis.Web.csproj"
COPY . .
WORKDIR "/src/SpotAnalysis.Web"
RUN dotnet build "./SpotAnalysis.Web.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./SpotAnalysis.Web.csproj" -c $BUILD_CONFIGURATION --no-restore -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SpotAnalysis.Web.dll"]
