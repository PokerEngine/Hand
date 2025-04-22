FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /usr/local/project/src

CMD dotnet watch --project Infrastructure run
