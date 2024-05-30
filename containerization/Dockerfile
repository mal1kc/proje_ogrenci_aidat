FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /app

# Copy csproj and restore dependencies
COPY ./OgrenciAidatSistemi/*.csproj ./
RUN dotnet restore

# Copy the rest of the application files
COPY ./OgrenciAidatSistemi/. ./

RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build-env /app/out .

RUN cat /etc/os-release && cat appsettings.json

EXPOSE 8080 8989
# Set the entry point for the application
ENTRYPOINT ["dotnet", "OgrenciAidatSistemi.dll"]

# docker/podman run -v /path/to/your/appsettings.json:/app/appsettings.json -p 8080:80 -d <image_name>
