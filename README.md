# ASP.NET Playground
This is a little fun project to try out some new technologies.  
In particular, I am making this to learn how to use ASP.NET and docker at the same time.

## Requirements
Dockers needs to be set up for this project to work.  
All the building and running the project will happen inside docker containers.  
Internet access is required first time to pull the necessary docker images.  

## Building
Run `./project.sh build` to build the project, 
or run `./project.sh rebuild` to clean-build it.  

## Code Generation
If you need to generate Code, or do anything else that requires the `dotnet` command,
you can run `./project.sh shell` to start a bash shell with dotnet installed (runs in docker).  
After you started the shell, the first command you should run is `dotnet restore`   
The tools `dotnet ef` and `dotnet aspnet-codegeneration` are also installed.

## Running
Run `./project.sh start` to start the default kestrel webserver.  
You can access the website on localhost:8080  
Stop the webserver using `./project.sh stop`  

If you want to quickly see your code-changes, you can run `./project.sh restart`.
That will stop the server, build the project, and then start the server again.
