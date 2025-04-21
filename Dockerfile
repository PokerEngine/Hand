FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /usr/local/project/src

# Copy solution and restore
COPY *.sln .
COPY Domain/*.csproj ./Domain/
COPY DomainTest/*.csproj ./DomainTest/
COPY Application/*.csproj ./Application/
COPY Infrastructure/*.csproj ./Infrastructure/
RUN dotnet restore

# Copy the entire project
COPY . .
WORKDIR /usr/local/project/src/Infrastructure
RUN dotnet publish -c Release -o /usr/local/project/app/publish

# Runtime image
FROM mcr.microsoft.com/dotnet/runtime:9.0
WORKDIR /usr/local/project/app
COPY --from=build /usr/local/project/app/publish .
ENTRYPOINT ["dotnet", "Infrastructure.dll"]
