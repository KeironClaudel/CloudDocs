# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["CloudDocs.API/CloudDocs.API.csproj", "CloudDocs.API/"]
COPY ["CloudDocs.Application/CloudDocs.Application.csproj", "CloudDocs.Application/"]
COPY ["CloudDocs.Domain/CloudDocs.Domain.csproj", "CloudDocs.Domain/"]
COPY ["CloudDocs.Infrastructure/CloudDocs.Infrastructure.csproj", "CloudDocs.Infrastructure/"]

RUN dotnet restore "CloudDocs.API/CloudDocs.API.csproj"

COPY . .

WORKDIR "/src/CloudDocs.API"
RUN dotnet publish "CloudDocs.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:10000
EXPOSE 10000

ENTRYPOINT ["dotnet", "CloudDocs.API.dll"]