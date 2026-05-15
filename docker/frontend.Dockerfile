FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

COPY frontend/GpuShare.Frontend.csproj ./
RUN dotnet restore GpuShare.Frontend.csproj

COPY frontend/. ./

RUN dotnet publish GpuShare.Frontend.csproj -c Release --no-restore -o /app/out

FROM mcr.microsoft.com/dotnet/aspnet:10.0

WORKDIR /app

COPY --from=build /app/out .

EXPOSE 8080
ENTRYPOINT ["dotnet", "GpuShare.Frontend.dll"]