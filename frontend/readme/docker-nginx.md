in this readme we will be talking about how nginx is integrated into docker workflow 

the common approach for angular app in docker container is to serve it on nginx 
but first, we must understand that angular app when build, is just html, css and javascript, i.e., static file. these files will be served by nginx and when client ask for it, nginx give it to client and client browser will run the static files. it is important to know that: nginx doesnt actually running the angular app, just a place to serve the static files

creating the docker file 
- its important to know that while some of the steps are the same with dockerfile for our backend server, there are a few notable differences
- we are also using multistage approach in this dockerfile
- the high view of the steps are as the following 
    - get build base image and setup the working directory 
    - copy package.json and package-lock.json 
    - run npm ci (this will remove node_modules and download exact version inside package.json)
    - copy necessary files for build 
    - run build 
    - end of stage 1 (build stage)

    - stage 2 (run stage)
    - get base image (for this, we'll be running the frontend on nginx inside alpine os - its a linux thing)
    - remove the default nginx page 
    - copy the files generated file during build into /usr/share/nginx/html
    - dont forget, index html must be inside this file because when building using npm build, index html is inside a subfolder browser 
    - copy the nginx config (we'll discuss this later) into /etc/nginx/ --> this will replace the files present in nginx 
    - expose the nginx on port 80 on docker container (this is for documentation)
    - to run the nginx, use CMD ["nginx", "-g", "daemon off;"]

then build the image docker build -t containerName:containerTag .

the nginx configuration. 
- nginx configuration is basically a config file of how our nginx should behave 
- nginx config file can be located in /etc/nginx/

understanding nginx.conf
- hierarchically has three parts, the global context, the events block and the http block
- un-included config will fallback to default value given by nginx, e.g, we want to omit the global context
- however, if we want to omit the events, make sure to include the event block and leave the block empty. 
- this is so that the config file still conform to the hierarchy. 
- mime (Multipurpose internet file extension) --> basically a list of types that nginx can serve

since our backend .net server running on docker container, the recommended way to access the backend server is by doing it on docker container layer 
while it is possible to access the backend through localhost:5000, we might encounter issues such as CORS that's making us can't access the backend server. 

so the clean and recommended approach is to access the backend through docker container layer 

to understand this we will be deep dive into docker compose 

what is docker compose 
- a docker-compose is a declarative yaml file that describe how multiple containers should be built, configured and connected and run together as a single application. 

