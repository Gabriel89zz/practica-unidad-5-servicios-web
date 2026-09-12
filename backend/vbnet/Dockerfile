# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copiar proyectos compartidos y proyecto VB.NET
COPY shared/WcfHelper ./shared/WcfHelper
COPY vbnet/AuditoriaApi.vbproj ./vbnet/
RUN dotnet restore ./vbnet/AuditoriaApi.vbproj

COPY vbnet ./vbnet
WORKDIR /src/vbnet
RUN dotnet publish AuditoriaApi.vbproj -c Release -o /app/publish

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8086
EXPOSE 8086

ENTRYPOINT ["dotnet", "AuditoriaApi.dll"]
