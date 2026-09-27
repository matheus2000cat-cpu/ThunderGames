FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

# Copia todo o código fonte para dentro do container
COPY . ./

# Restaura e publica diretamente o projeto da API
WORKDIR /app/ThunderGames.API
RUN dotnet restore ThunderGames.API.csproj
RUN dotnet publish -c Release -o /out

# Estágio de Execução
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /out .

ENV ASPNETCORE_URLS=http://+:$PORT
EXPOSE 8080

ENTRYPOINT ["dotnet", "ThunderGames.API.dll"]