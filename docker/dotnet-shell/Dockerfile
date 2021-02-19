FROM mcr.microsoft.com/dotnet/sdk:5.0

# Entity Framework (ORM) ; does database stuff for us
RUN dotnet tool install --global dotnet-ef
# Code Generation ; helps make Controllers etc for us
RUN dotnet tool install --global dotnet-aspnet-codegenerator

ENV PATH="$PATH:/root/.dotnet/tools"