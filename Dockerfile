FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

COPY ./lib/pokerstove /usr/local/lib/pokerstove

RUN set -x && \
    apt-get update && \
    apt-get install -y \
        git \
        build-essential \
        libboost-all-dev \
        cmake && \
    rm -rf /var/lib/apt/lists/* && \
    cd /usr/local/lib/pokerstove && \
    cmake -DCMAKE_BUILD_TYPE=Release -S \. -B build && \
    cmake --build build --target all test -j 4

WORKDIR /usr/local/project

CMD ["dotnet", "watch", "--project", "src/Infrastructure", "run"]
