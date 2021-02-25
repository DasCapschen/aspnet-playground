# ASP.NET Playground
This is a little fun project to try out some new technologies.  
In particular, I am making this to learn how to use ASP.NET and docker at the same time.

## Requirements
Dockers needs to be set up for this project to work.  
All the building and running the project will happen inside docker containers.  
Internet access is required first time to pull the necessary docker images.  

## NEW WORKFLOW
You need to install the vscode extension [remote-containers](https://marketplace.visualstudio.com/items?itemName=ms-vscode-remote.remote-containers).  
It should then automatically find the files in .devcontainer folder and ask you if you want to open the project in the docker container, do so.  
Now you can open a new terminal in vs code and access the command line to build the project etc. (or launch through vscode, but I didn't test if launch.json works)  

To build: `./project.sh build` or `./project.sh rebuild`  
To run: `./project.sh start`  
To stop: ctrl+c, you're using a terminal ;p  
(project.sh will automatically figure out if it is runnning in a dev container or locally)  

dotnet ef and dotnet aspnet-codegeneration are installed in the devcontainer.

You can access the website on localhost:8182

## OLD WORKFLOW
### Building
Run `./project.sh build` to build the project, 
or run `./project.sh rebuild` to clean-build it.  

### Code Generation
If you need to generate Code, or do anything else that requires the `dotnet` command,
you can run `./project.sh shell` to start a bash shell with dotnet installed (runs in docker).  
After you started the shell, the first command you should run is `dotnet restore`   
The tools `dotnet ef` and `dotnet aspnet-codegeneration` are also installed.

### Running
Run `./project.sh start` to start the default kestrel webserver.  
You can access the website on localhost:8182  
Stop the webserver using `./project.sh stop`  

If you want to quickly see your code-changes, you can run `./project.sh restart`.
That will stop the server, build the project, and then start the server again.
