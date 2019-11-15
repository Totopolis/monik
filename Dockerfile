#Depending on the operating system of the host machines(s) that will build or run the containers, the image specified in the FROM statement may need to be changed.
#For more information, please see https://aka.ms/containercompat

FROM mcr.microsoft.com/dotnet/core/aspnet:3.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:3.0 AS publish
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