FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Learnify.Core/Learnify.Core.csproj", "Learnify.Core/"]
COPY ["Learnify.Registration.API/Learnify.Registration.API.csproj", "Learnify.Registration.API/"]
RUN dotnet restore "Learnify.Registration.API/Learnify.Registration.API.csproj"
COPY . .
WORKDIR "/src/Learnify.Registration.API"
RUN dotnet build "Learnify.Registration.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Learnify.Registration.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Learnify.Registration.API.dll"]
