# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# Copy csproj files only
COPY LinearProgrammingWeb/LinearProgrammingWeb.csproj LinearProgrammingWeb/
COPY SimplexMethod/SimplexMethod.csproj SimplexMethod/

# Restore dependencies (only necessary projects)
RUN dotnet restore LinearProgrammingWeb/LinearProgrammingWeb.csproj

# Copy the rest of the source code
COPY . ./

# Publish the web app
RUN dotnet publish LinearProgrammingWeb/LinearProgrammingWeb.csproj -c Release -o /app/publish

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 80
ENTRYPOINT ["dotnet", "LinearProgrammingWeb.dll"]

ENV ASPNETCORE_URLS=http://+:80
