FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Learnify.Core/Learnify.Core.csproj", "Learnify.Core/"]
COPY ["Learnify.Exams.API/Learnify.Exams.API.csproj", "Learnify.Exams.API/"]
RUN dotnet restore "Learnify.Exams.API/Learnify.Exams.API.csproj"
COPY . .
WORKDIR "/src/Learnify.Exams.API"
RUN dotnet build "Learnify.Exams.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Learnify.Exams.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Learnify.Exams.API.dll"]
