FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ["DnDMapBuilder.sln", "./"]
COPY ["src/DnDMapBuilder.Api/DnDMapBuilder.Api.csproj", "src/DnDMapBuilder.Api/"]
COPY ["src/DnDMapBuilder.Application/DnDMapBuilder.Application.csproj", "src/DnDMapBuilder.Application/"]
COPY ["src/DnDMapBuilder.Contracts/DnDMapBuilder.Contracts.csproj", "src/DnDMapBuilder.Contracts/"]
COPY ["src/DnDMapBuilder.Data/DnDMapBuilder.Data.csproj", "src/DnDMapBuilder.Data/"]

# Restore dependencies
RUN dotnet restore "src/DnDMapBuilder.Api/DnDMapBuilder.Api.csproj"

# Copy everything else
COPY . .

# Build
WORKDIR "/src/src/DnDMapBuilder.Api"
RUN dotnet build "DnDMapBuilder.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DnDMapBuilder.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DnDMapBuilder.Api.dll"]
