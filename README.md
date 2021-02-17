# ASP.NET Playground
This is a little fun project to try out some new technologies.  
In particular, I am making this to learn how to use ASP.NET and docker at the same time.

## Requirements
Dockers needs to be set up for this project to work.  
All the building and running the project will happen inside docker containers.  
Internet access is required first time to pull the necessary docker images.  

## Building
Run `./project.sh build`  

## Running
Run `./project.sh start` to start the default kestrel webserver.  
You can access the website on localhost:8080  
Stop the webserver using `./project.sh stop`
