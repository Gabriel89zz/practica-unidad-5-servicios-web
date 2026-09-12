# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY FacturacionApi.csproj .
RUN dotnet restore FacturacionApi.csproj
COPY . .
RUN dotnet publish FacturacionApi.csproj -c Release -o /app/publish

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8085
EXPOSE 8085

ENTRYPOINT ["dotnet", "FacturacionApi.dll"]
