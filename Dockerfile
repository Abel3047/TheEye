# Stage 1: Build the application
# Use the official .NET 8 SDK image to build our app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /src

# Copy the project files and restore dependencies
COPY . .
RUN dotnet restore

# Publish the application, creating a release build
# IMPORTANT: If your main web project is in a subfolder, you might need to change this path.
# For example: RUN dotnet publish "MyWebApp/MyWebApp.csproj" -c Release -o /app/publish
RUN dotnet publish -c Release -o /app/publish

# Stage 2: Create the final, smaller runtime image
# Use the official ASP.NET 8 runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# Copy the published output from the build stage
COPY --from=build-env /app/publish .

# The entrypoint for the container. This command runs your app.
# IMPORTANT: Replace 'TheEye.dll' with the actual name of your web project's DLL file.
# You can find this in your project's bin/Release/net8.0 folder after publishing.
ENTRYPOINT ["dotnet", "TheEye.dll"]