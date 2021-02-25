#!/bin/bash

build() {
    if [ $IS_DEV_CONTAINER = 1 ]; then
        dotnet build src/src.csproj
        dotnet publish src/src.csproj
    else
        #build the image (see Dockerfile)
        #target is run (we have 2 stages, run and build)
        #name the created image "aspnet-playground"
        docker build --target run -t aspnet-playground -f ./docker/aspnet-playground/Dockerfile .
    fi
}

rebuild() {
    if [ $IS_DEV_CONTAINER = 1 ]; then
        dotnet clean src/src.csproj
        build
    else
        docker build --target run -t aspnet-playground --no-cache -f ./docker/aspnet-playground/Dockerfile .
    fi
}

start() {
    if [ $IS_DEV_CONTAINER = 1 ]; then
        dotnet run --project src/src.csproj
    else
        #run in background (-d)
        #forward HOST port 8182 to CONTAINER port 80
        #run image called "aspnet-playground"
        #remove container when stopped (--rm)
        #docker run --name aspnet-playground --rm -p 8182:80 -d aspnet-playground
        docker-compose -f ./docker/docker-compose.yml up -d web db
    fi
}

stop() {
    #no need inside dev container, just use CTRL+C, you're running from terminal anyways :)

    #docker stop aspnet-playground
    docker-compose -f ./docker/docker-compose.yml down
}

shell() {
    #unnecessary when using dev container :)

    docker build --build-arg UID=$(id -u) --build-arg GID=$(id -g) -t aspnet-playground-dotnet-shell -f ./docker/dotnet-shell/Dockerfile .
    docker-compose -f ./docker/docker-compose.yml run --rm dotnet_shell
}

export COMPOSE_DOCKER_CLI_BUILD=1
export DOCKER_BUILDKIT=1

PROJECT_ROOT=$(cd $(dirname "$0") && pwd)
cd $PROJECT_ROOT

# letting docker-compose or similar pull the images results in a "connection refused"
# for some reason, so we have to explicitly pull the images before
if ! [ $IS_DEV_CONTAINER = 1 ]; then
    if [[ "$(docker images -q mcr.microsoft.com/dotnet/sdk:5.0 2> /dev/null)" == "" ]]; then
    docker pull mcr.microsoft.com/dotnet/sdk:5.0
    fi

    if [[ "$(docker images -q mcr.microsoft.com/dotnet/sdk:5.0 2> /dev/null)" == "" ]]; then
    docker pull mcr.microsoft.com/dotnet/aspnet:5.0
    fi

    if [[ "$(docker images -q postgres:13.2-alpine 2> /dev/null)" == "" ]]; then
    docker pull postgres:13.2-alpine
    fi
fi

case "$1" in
    build)
        build
        ;;
    rebuild)
        rebuild
        ;;
    start)
        start
        ;;
    restart)
        stop
        build
        start
        ;;
    stop)
        stop
        ;;
    shell)
        shell
        ;;
    *)
        echo "usage: ./project.sh COMMAND"
        echo "    build      builds the project"
        echo "    rebuild    cleans and builds the project"
        echo "    start      run the webserver"
        echo "    restart    stop, build, start"
        echo "    stop       stop the webserver"
        echo "    shell      start a shell in docker that has dotnet installed"
        ;;
esac