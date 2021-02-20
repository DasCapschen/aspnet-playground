FROM mcr.microsoft.com/dotnet/sdk:5.0

ARG UID
ARG GID

RUN groupadd -f -g $GID container
RUN useradd -u $UID -g $GID -m -s /bin/bash docker

USER ${UID}:${GID}

# Entity Framework (ORM) ; does database stuff for us
RUN dotnet tool install --global dotnet-ef
# Code Generation ; helps make Controllers etc for us
RUN dotnet tool install --global dotnet-aspnet-codegenerator

ENV PATH="$PATH:/home/docker/.dotnet/tools"