# Paso 1: Construye el proyecto
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copia el proyecto y resuelve las dependencias
COPY ["CrudPedido.csproj", "./"]
RUN dotnet restore "CrudPedido.csproj"

# Copia el resto del codigo
COPY . .
RUN dotnet publish "CrudPedido.csproj" -c Release -o /app/publish

# Paso 2: Prepara aspnet
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Asigna el puerto
EXPOSE 5080
ENV ASPNETCORE_URLS=http://0.0.0.0:5080

# Copia el binario compilado
COPY --from=build /app/publish .

# Ejecuta la app
ENTRYPOINT ["dotnet", "CrudPedido.dll"]
