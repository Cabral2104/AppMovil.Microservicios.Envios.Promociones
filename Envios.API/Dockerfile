# Etapa de build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copiar archivos de proyecto y restaurar dependencias
COPY ["Envios.API/Envios.API.csproj", "Envios.API/"]
COPY ["Envios.Domain/Envios.Domain.csproj", "Envios.Domain/"]
COPY ["Envios.Infrastructure/Envios.Infrastructure.csproj", "Envios.Infrastructure/"]

RUN dotnet restore "Envios.API/Envios.API.csproj"

# Copiar todo el código y compilar
COPY . .
WORKDIR "/src/Envios.API"
RUN dotnet publish "Envios.API.csproj" -c Release -o /app/publish

# Etapa de runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Envios.API.dll"]