FROM mcr.microsoft.com/dotnet/core/aspnet:3.1 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS publish
ARG VERSION=1.0.0
WORKDIR /src
COPY . .
RUN dotnet publish "src/Monik.Service/Monik.Service.csproj" \
    -c Release /p:Version=${VERSION} \
    -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Monik.Service.dll"]