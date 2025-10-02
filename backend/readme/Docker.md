DOCKER 🐳

what is docker 
- docker is a tool that solves it runs on my machine problem 
- basically bundle up all deps, runtime, etc, into a file called image and run it inside a virtual machine called docker container 

key elements
- dockerfile 
- docker image 
- docker container 

what to do before: 
install docker-desktop. this will also install the command line tool, and stuff you need to work with docker 

DOCKERFILE 
- a docker file is a step by step instruction to build docker image. 
- the instruction is to be carried out by the docker engine. 
- the output from dockerfile is a docker image 

DOCKER IMAGE 
- docker image contains the files needed to run our app
- dependencies, runtime, etc... 
- docker image is then loaded into a container and tada... we have our app running inside a docker container 

DOCKER CONTAINER 
- can view this one as a vm that runs our app
- has its own port to listen to request 
- has its own file system, terminal, etc...
- after all its a trimmed OS just enough to run our app

environment files 
- sensitive data like connectionstring, api keys is not baked into the docker image 
- env configuration is to be provided when want to run a docker container 
- there a few ways to give env config to a container 
    - through terminal use the -e param, e.g., 
        docker run -e "the sensitive data" 
        note: when have nested data like: 
        {
            connectionString: {
                DefaultConnection: "the string"
            }
        }
        use __ to serve the nested object. e.g., ConnectionString__DefaultConnection=thestring
    - through predefined env file 
        create a .env file (dont forget to gitignore)
        load during docker run 
        docker run -d --env-file .env (-d means detach: to run the container in foreground, not in terminal)
        also use __ for nested value 
    - docker-compose.yaml
        also leverage the use of .env file, but we will discuss this some other time 

WRITING DOCKER FILE 
- there are structure when writing docker file 
- for best practice, we will be using stage dockefile, means the instruction is stage by stage 
- stage 1: the build stage 
    FROM <sdk> as build
        this will download the sdk that we want to build our app with, in this project, it is dotnet sdk version 9 so
        mcr.microsoft.com/dotnet/sdk:9.0 (mcr: microsoft container registry)
    WORK <dir> 
        this will set the working directory of docker engine, can think of it as cd <path>
    COPY [<source>, <destination>] 
        this will copy stuff from the local machine (build context) into the container filesystem 
        in this case we only want to copy csproj for api, core and infrastructure. this is because we want to run dotnet restore
        so in this case
    COPY [] api project, remember only csproj
    COPY [] core project 
    COPY [] infrastructure 

    RUN dotnet restore "api.csproj" 
        this one is to get the dependencies listed in the csprojs files 
    
    WORK <api-dir> 
        go into api directory that we copied just now 

    RUN dotnet build <api-csproj> -c Release -o /app/build 
        this one build the app and store it inside /app/build 
        -c stands for config, default is Debug and this time we want release 
        -o is output, duhhh
    
    RUN dotnet publish <api-csproj> -c Release -o /app/publish
        the same thing but this one is publish, stripped out version
    
- stage 2: runtime 
    now that we have our app built, then we can move into runtime instruction 
    FROM <asp-dotnet-runtime> as final 
        notice that we dont use dotnet here but uses asp runtime
        because we only need the runtime to run the app
        its actually the stripped out version of dotnet and asp runtime, just enough to run our app
    WORK <runtime-dir> 
        move into new dir for runtime stage so that our file is clean 
    COPY --from=build /app/publish .
        copy actually has signature of COPY --from=<stage-name-or-index> [<source>, <destination>]
        get the stuff from inside build after we run dotnet publish earlier 
    ENTRYPOINT [<executable>, <param>]
        concat executable and param, e.g., ["dotnet", "api.dll"] will run dotnet api.dll (which is a way to start out app server)
        this ENTRYPOINT happens first thing the container is init'd
    CMD [<executable>, <param>]
        same with entrypoint, but is overridable, meaning when you docker run, you can override the value thats written here 

CREATING DOCKER IMAGE FROM DOCKER FILE 
- use this command and make sure to be inside the same dir as dockerfile located 
    docker build -t <name>:<tag>
        -t stands for tag 
    