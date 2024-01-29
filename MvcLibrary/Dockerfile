FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["MvcLibrary/MvcLibrary.csproj", "MvcLibrary/"]
RUN dotnet restore "MvcLibrary/MvcLibrary.csproj"
COPY . .
WORKDIR "/src/MvcLibrary"
RUN dotnet build "MvcLibrary.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MvcLibrary.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MvcLibrary.dll"]
