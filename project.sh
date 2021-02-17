#!/bin/bash

export DOCKER_BUILDKIT=1

cd $(dirname "$0")

case "$1" in
    build)
        #build the image (see Dockerfile)
        #target is run (we have 2 stages, run and build)
        #name the created image "aspnet-playground"
        docker build --target run -t aspnet-playground .
        ;;
    rebuild)
        docker build --target run -t aspnet-playground --no-cache .
        ;;
    start)
        #run in background (-d)
        #forward HOST port 8080 to CONTAINER port 80
        #run image called "aspnet-playground"
        #remove container when stopped (--rm)
        docker run --name aspnet-playground --rm -p 8080:80 -d aspnet-playground
        ;;
    stop)
        docker stop aspnet-playground
        ;;
    shell)
        docker-compose run dotnet_shell
        ;;
    *)
        echo "usage: ./project.sh COMMAND"
        echo "    build      builds the project"
        echo "    rebuild    cleans and builds the project"
        echo "    start      run the project"
        echo "    stop       stop the project"
        echo "    shell      start a shell in docker that has dotnet installed"
        ;;
esac