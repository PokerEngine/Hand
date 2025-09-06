FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

COPY ./lib/pokerstove /usr/local/lib/pokerstove
COPY ./lib/pokerstove-ext/src/programs/ps-recognize /usr/local/lib/pokerstove/src/programs/ps-recognize

RUN set -x && \
    apt-get update && \
    apt-get install -y \
        build-essential \
        libboost-all-dev \
        cmake && \
    rm -rf /var/lib/apt/lists/* && \
    cd /usr/local/lib/pokerstove && \
    echo "add_subdirectory (ps-recognize)\n" >> ./src/programs/CMakeLists.txt && \
    cmake -DCMAKE_BUILD_TYPE=Release -S \. -B build && \
    cmake --build build --target all test -j 4

WORKDIR /usr/local/hand

CMD ["dotnet", "watch", "--project", "src/Infrastructure", "run"]
