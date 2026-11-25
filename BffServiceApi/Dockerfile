# Build a partir desta pasta:  docker build -t branchstorm-bff .
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY BffServiceApiClassLibrary/BffApiClassLibrary.csproj BffServiceApiClassLibrary/
COPY BffServiceApi/BffApi.csproj BffServiceApi/
RUN dotnet restore BffServiceApi/BffApi.csproj
COPY . .
RUN dotnet publish BffServiceApi/BffApi.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app .
# Render injeta PORT; localmente cai em 5030
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 5030
ENTRYPOINT ["sh", "-c", "ASPNETCORE_URLS=http://+:${PORT:-5030} dotnet BffServiceApi.dll"]
