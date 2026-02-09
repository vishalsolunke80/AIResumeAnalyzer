# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["AIResumeAnalyzer.csproj", "./"]
RUN dotnet restore "AIResumeAnalyzer.csproj"
COPY . .
RUN dotnet build "AIResumeAnalyzer.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "AIResumeAnalyzer.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Install dependencies for SQLite if necessary (usually built-in)
# Create an empty DB file so the app can migrate it if it doesn't exist
# Note: Data will be lost on restart in Render Free Tier
RUN touch ResumeAnalyzer.db

ENV ASPNETCORE_URLS=http://+:10000
EXPOSE 10000

ENTRYPOINT ["dotnet", "AIResumeAnalyzer.dll"]
