# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj and restore as distinct layers
COPY ["LUPIAN_Activity.csproj", "./"]
RUN dotnet restore "LUPIAN_Activity.csproj"

# Copy everything else and build
COPY . .
RUN dotnet build "LUPIAN_Activity.csproj" -c Release -o /app/build

# Publish Stage
FROM build AS publish
RUN dotnet publish "LUPIAN_Activity.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final Runtime Stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Render uses the PORT environment variable
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "LUPIAN_Activity.dll"]
