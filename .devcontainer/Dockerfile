# See here for image contents: https://github.com/microsoft/vscode-dev-containers/tree/v0.158.0/containers/dotnet/.devcontainer/base.Dockerfile

# [Choice] .NET version: 5.0, 3.1, 2.1
ARG VARIANT="5.0"
FROM mcr.microsoft.com/vscode/devcontainers/dotnetcore:0-${VARIANT}

# don't install anything extra plz
# (the same args exist in the FROM image, so not sure if we need to respecify here)
ARG INSTALL_NODE="false"
ARG INSTALL_AZURE_CLI="false"

#added by the base image, hope this works
USER vscode

# copy ssh key (docker secret) into container
# allows using git from container
RUN mkdir -p $HOME/.ssh && ln -s /run/secrets/git_ssh_key $HOME/.ssh/id_rsa

ENV DOTNET_CLI_TELEMETRY_OPTOUT=1

# Entity Framework (ORM) ; does database stuff for us
RUN dotnet tool install --global dotnet-ef
# Code Generation ; helps make Controllers etc for us
RUN dotnet tool install --global dotnet-aspnet-codegenerator
# make installed tools available
# apprently $HOME is not set :rolling_eyes:
ENV PATH="$PATH:/home/vscode/.dotnet/tools"

# used for project.sh to determine new vs old workflow :)
ENV IS_DEV_CONTAINER=1