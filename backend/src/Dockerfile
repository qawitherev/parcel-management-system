# get the base image with sdk 
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

# make working dir for container 
WORKDIR /src

# copy relevant csproj 
COPY ["ParcelManagement.Api/ParcelManagement.Api.csproj", "ParcelManagement.Api/"]
COPY ["ParcelManagement.Core/ParcelManagement.Core.csproj", "ParcelManagement.Core/"]
COPY ["ParcelManagement.Infrastructure/ParcelManagement.Infrastructure.csproj", "ParcelManagement.Infrastructure/"]

# dotnet restore api, no need to restore other project since 
# it is already referenced inside api.csproj
RUN dotnet restore "ParcelManagement.Api/ParcelManagement.Api.csproj"

# copy rest of the source from machine to container file system
COPY ParcelManagement.Api/ ParcelManagement.Api/
COPY ParcelManagement.Core/ ParcelManagement.Core/
COPY ParcelManagement.Infrastructure/ ParcelManagement.Infrastructure/

# set working dir to api, since api is our entry point 
WORKDIR /src/ParcelManagement.Api

# run dotnet build 
RUN dotnet build "ParcelManagement.Api.csproj" -c Release -o /app/build --no-restore 

# run publish 
RUN dotnet publish "ParcelManagement.Api.csproj" -c Release -o /app/publish --no-restore 

# move to runtime stage 
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final

# new stage we want new dir, so we don't clutter the file 
WORKDIR /app

COPY --from=build /app/publish .

ENTRYPOINT [ "dotnet", "ParcelManagement.Api.dll"]
CMD [ "--urls", "http://*:5163" ]



