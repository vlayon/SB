FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Core/Core.csproj", "Core/"]
COPY ["Data/Data.csproj", "Data/"]
COPY . .
RUN dotnet restore "Core/Core.csproj"
RUN dotnet publish "Core/Core.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
# runtime env vars are provided at docker run / orchestrator level
ENTRYPOINT ["dotnet", "Core.dll"]