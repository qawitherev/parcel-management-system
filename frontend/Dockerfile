# build stage using nginx running on alpine
FROM node:20-alpine AS build 

# set the working directory 
WORKDIR /app

# copy necessary files for build 
COPY package-lock.json package.json ./

# run the npm ci (getting the dependencies, by deleting existing node-modules, and downloading new one with version stated in package.json
RUN npm ci 

# copy necessary files copilot review plis help me choose what file to choose, but for this time, ill copy all
COPY . .

# run the build command with prod configuration
RUN npm run build --production

# now that we have the thing, lets move to stage 2, serve the thing inside nginx 
FROM nginx:alpine AS final 

# sidenote, when we start new stage, the file system from previous stage is not available in this stage
# tldr, each stage file system is isolated from each other 

# remove default nginx html 
RUN rm -rf /usr/share/nginx/html/*

# copy build output from build stage 
# /usr/share/nginx/html is the the default if we want to expose to nginx 
COPY --from=build /app/dist/frontend/browser /usr/share/nginx/html
COPY --from=build /app/dist/frontend/3rdpartylicenses.txt /usr/share/nginx/html
COPY --from=build /app/dist/frontend/prerendered-routes.json /usr/share/nginx/html

# for documentation purposes 
EXPOSE 80

# serve the file by starting nginx 
# we use CMD to give more flexibility on runtime 
# also nginx use it, so just follow 
CMD [ "nginx", "-g", "daemon off;" ]

# -g is the entrypoint so we can give daemon off; command to the nginx
# typically daemon off; command is defined inside a config file, but using -g we can just give it inside the cli
# daemon off meaning run the nginx process in foreground, because docker expect the main process 
# to be running at the front
# if the process if dead, container will shutdown 



