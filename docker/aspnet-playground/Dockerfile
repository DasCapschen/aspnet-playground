# BUILD
# use dotnet sdk image
FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src

#copy project file and restore
#COPY ./src/*.csproj .
#RUN dotnet restore

#copy rest and build
#using --no-restore here completely breaks things... because M$ or something
COPY ./src/. .
#RUN dotnet build -o /build
#because build messes up CSS and JS... /)_-
RUN dotnet publish -o /build

# RUN
# use aspnet runtime image
FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS run
WORKDIR /build
COPY --from=build /build .
ENV ASPNETCORE_ENVIRONMENT=Development
ENTRYPOINT ["dotnet", "src.dll"]