# Estágio de Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

# Copia a solução e os arquivos de projeto
COPY *.sln ./
COPY ThunderGames.Domain/*.csproj ThunderGames.Domain/
COPY ThunderGames.Data/*.csproj ThunderGames.Data/
COPY ThunderGames.API/*.csproj ThunderGames.API/

# Restaura as dependências
RUN dotnet restore

# Copia todo o restante do código fonte
COPY . ./

# Publica a API em modo Release
WORKDIR /app/ThunderGames.API
RUN dotnet publish -c Release -o /out

# Estágio de Execução
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /out .

# Configura a porta padrão que o Render exige
ENV ASPNETCORE_URLS=http://+:$PORT
EXPOSE 8080

ENTRYPOINT ["dotnet", "ThunderGames.API.dll"]