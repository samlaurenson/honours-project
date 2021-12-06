FROM mcr.microsoft.com/dotnet/aspnet:latest AS base
FROM mcr.microsoft.com/dotnet/core/sdk:latest AS build
WORKDIR /HonoursProject

COPY HonoursProject/ ./HonoursProject/
COPY HonoursProjectTests/ ./HonoursProjectTests/

RUN dotnet restore ./HonoursProject/HonoursProject.csproj

# run tests on docker build
RUN dotnet test ./HonoursProjectTests/HonoursProjectTests.csproj

RUN dotnet publish ./HonoursProject/HonoursProject.sln -c Release

# run tests on docker run
WORKDIR /HonoursProject
COPY HonoursProject/bin/Release/netcoreapp3.1/publish .
ENTRYPOINT ["dotnet", "HonoursProject.dll"]