*this is still a draft of how i am going to write this documentation*

* intro to what is cicd
  * Continuous integration continous deployment is a strategy to get small incremental changes quickly getting implemented one step at a time. With this quick small incremental changes, we can quickly deploy or revert the changes from our product. This enables the product to stay updated and ahead of its competitor.

- what problem we are solving

* my cicd strategy

  * everything runs on container
  * branching strategy [active branch, development, staging, production]
  * explain what each branch is for
  * active branch -> this is where most development and frequent code changes happen
  * development -> when development already completed, push into this branch. unit test and linting will happen here
  * staging -> no test happen here maybe we will include static code analysis here. just code analysis, because we already tested when pushed to development. here we will build docker image and run the container, locally. even the pipeline runs locally.
  * production -> this is where the production code is.
    * In this project, we are using 3 main branches and one development branch. Namely, main, staging and development; and another one active branch. Active branch is where most of the code is written for development purposes.
    * Development branch is for storing tested and linted code. There is no deployment happen here. Just storing tested code. Developer will run the code they write using development tools such as dotnet run or ng serve.
    * When code is pushed into staging branch after pr is approved, git action will run runner locally, build docker image and run docker container locally. The purpose of having this step is to verify that build docker script is working correctly as intended.
    * Production branch is self explanatory. But instead of having locally, the pipeline will use cloud runner and will be deployed to cloud aws infrastructure defined using terraform.
* talk about docker

  * what is it and what problem does it solve (why we use docker)
  * how we use docker
    * have 2 images, frontend and backend
  * where we run docker?
    * on pr staging
    * on pr main
  * Docker is a technology that makes your project to be bundled up inside a single image called docker image that can be run inside a docker container. Inside the image already have the compiled code, the runtime and the dependencies. This greatly enhances portability because any machine can run the app without having to manually setup the runtime and dependencies. 
  * In this project, we are using two images, frontend and backend image. Frontend will have the angular app served on nginx and backend will have the dotnet runtime and our compiled backend code. 
* git action

  * what is git action
  * how git action works under the hood
  * why we use git action -> to automate stuff
  * how we use git action
    * cloud runner
    * self hosted runner
* aws services used

  * explain our ecs architecture (ecs, fargate, service, task)
  * why this architecture
  * future improvements
* terraform

  * what is terraform?
  * what problem does terraform solve
  * how we use terraform
  * terraform module pattern
* integrating terraform into git action

  * why do we need to use terraform along with git action
  * how to we integrate it
  * possible future enhancement to make it more efficient, e.g, cache the terraform module

---
