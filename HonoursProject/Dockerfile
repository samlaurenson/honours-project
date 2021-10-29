FROM mcr.microsoft.com/dotnet/aspnet:5.0
COPY bin/Release/netcoreapp3.1/publish HonoursProject/
WORKDIR /HonoursProject
ENTRYPOINT ["dotnet", "HonoursProject.dll"]