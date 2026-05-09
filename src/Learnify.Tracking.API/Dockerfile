FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Learnify.Core/Learnify.Core.csproj", "Learnify.Core/"]
COPY ["Learnify.Tracking.API/Learnify.Tracking.API.csproj", "Learnify.Tracking.API/"]
RUN dotnet restore "Learnify.Tracking.API/Learnify.Tracking.API.csproj"
COPY . .
WORKDIR "/src/Learnify.Tracking.API"
RUN dotnet build "Learnify.Tracking.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Learnify.Tracking.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Learnify.Tracking.API.dll"]
