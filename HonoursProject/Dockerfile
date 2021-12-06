FROM mcr.microsoft.com/dotnet/aspnet:latest AS base
FROM mcr.microsoft.com/dotnet/core/sdk:latest AS build
COPY bin/Release/netcoreapp3.1/publish HonoursProject/
WORKDIR /HonoursProject
RUN dotnet restore
RUN dotnet publish -c Release -o out
CMD ["dotnet", "test"]
ENTRYPOINT ["dotnet", "HonoursProject.dll"]