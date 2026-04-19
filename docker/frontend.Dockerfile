FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /app

COPY frontend/GpuShare.Frontend ./

RUN dotnet restore
RUN dotnet build -c Release --no-restore
RUN dotnet publish -c Release --no-restore -o /app/out

FROM mcr.microsoft.com/dotnet/aspnet:10.0

WORKDIR /app

COPY --from=build /app/out .

EXPOSE 8080
ENTRYPOINT ["dotnet", "GpuShare.Frontend.dll"]