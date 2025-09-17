# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 443

# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["eventos_qr/eventos_qr.csproj", "eventos_qr/"]
RUN dotnet restore "./eventos_qr/eventos_qr.csproj"
COPY . .
WORKDIR "/src/eventos_qr"
RUN dotnet build "./eventos_qr.csproj" -c Release -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
RUN dotnet publish "./eventos_qr.csproj" -c Release -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
ENV TZ=America/Bogota
ENV DEBIAN_FRONTEND=noninteractive
RUN apt-get update && apt-get install -y nano && apt-get install -y tzdata
WORKDIR /app
COPY --from=publish /app/publish .
RUN ulimit -n 65536
ENTRYPOINT ["dotnet", "eventos_qr.dll"]