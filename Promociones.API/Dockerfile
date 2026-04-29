FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["Promociones.API/Promociones.API.csproj", "Promociones.API/"]
COPY ["Promociones.Domain/Promociones.Domain.csproj", "Promociones.Domain/"]
COPY ["Promociones.Infrastructure/Promociones.Infrastructure.csproj", "Promociones.Infrastructure/"]

RUN dotnet restore "Promociones.API/Promociones.API.csproj"

COPY . .
WORKDIR "/src/Promociones.API"
RUN dotnet publish "Promociones.API.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Promociones.API.dll"]