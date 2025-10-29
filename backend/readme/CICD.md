# CI/CD Documentation

## Introduction to CI/CD

Continuous Integration and Continuous Deployment (CI/CD) is a software development strategy that enables small, incremental changes to be implemented and deployed quickly. With rapid incremental changes, I can deploy or revert changes from the product efficiently. This approach helps keep the product updated and competitive.

## CI/CD Strategy

### Branching Strategy

I use a multi-branch workflow with the following branches:

- **Active branch** - Where most development and frequent code changes occur
- **Development** - Stores tested and linted code after development is complete. Unit tests and linting run when code is pushed here. No deployment occurs at this stage
- **Staging** - Code analysis happens here. I build Docker images and run containers locally using a self-hosted runner to verify the Docker build scripts work correctly
- **Main (Production)** - Contains production-ready code. The pipeline uses GitHub-hosted runners to deploy to AWS infrastructure defined with Terraform

During development, I run code locally using development tools such as `dotnet run` or `ng serve`. After a pull request is approved and merged to staging, GitHub Actions triggers a self-hosted runner to build Docker images and run containers locally. When code is merged to main, the pipeline deploys to AWS cloud infrastructure.

## Docker

### What is Docker?

Docker is a containerization platform that bundles an application, its runtime, and dependencies into a single image that can be run in a container. This enhances portability because any machine can run the application without manually setting up the runtime and dependencies.

### Docker in This Project

I use two Docker images:

- **Frontend image** - Contains the Angular application served by NGINX
- **Backend image** - Contains the .NET runtime and compiled backend code

### Docker Usage

**Staging environment:**
- Docker images are built and tagged with `staging` and `github.sha`
- Docker Compose starts both frontend and backend containers locally on a self-hosted runner

**Production environment:**
- Docker images are built and tagged with `github.sha` and `latest`
- Images are pushed to DockerHub (a remote Docker registry)
- AWS Elastic Container Service (ECS) on Fargate pulls images from DockerHub during deployment
- Terraform automatically provisions the required AWS infrastructure

## GitHub Actions

### What is GitHub Actions?

GitHub Actions is a CI/CD platform maintained by GitHub that automates workflows by running commands inside virtual machines called runners. Workflows are defined declaratively in YAML files placed in the `.github/workflows` folder. Jobs within a workflow run in parallel unless dependencies are explicitly specified, while steps within a job execute sequentially.

### Runner Types

**GitHub-hosted runners** - Run on GitHub's cloud infrastructure

**Self-hosted runners** - Run on local machines after being registered with GitHub

In this project, I use self-hosted runners for staging deployments and GitHub-hosted runners for production deployments to AWS.

## AWS Services

### Architecture Overview

I use AWS to host the production server. Since this is a containerized application, I chose AWS Elastic Container Service (ECS) as the hosting platform. To reduce costs, I pull Docker images from DockerHub instead of storing them in AWS Elastic Container Registry (ECR).

### ECS Components

**Fargate** - A serverless compute engine for containers. Unlike EC2, Fargate eliminates the need to manage virtual machines, security patching, and Docker installation. This reduces operational overhead at a slightly higher cost.

**Task Definition** - Defines the containers to run, similar to a pod in Kubernetes. In this project, both frontend and backend containers run in the same task.

**Service** - Manages the task instances. I configured the service with a desired count of 1, meaning one instance of the task runs at any given time.

This simplified architecture is suitable for the initial deployment. Future improvements could include separating frontend and backend into different services, implementing auto-scaling, and using a load balancer.

## Terraform

### What is Terraform?

Terraform is an Infrastructure as Code (IaC) tool that allows infrastructure to be defined and managed through code rather than manually through the AWS Console or AWS CLI. This is essential for automated CI/CD pipelines because it eliminates manual provisioning, reduces human error, and ensures consistency.

### Terraform Workflow

Terraform achieves idempotency through the following workflow:

1. **Plan** - Terraform reads the configuration files, queries AWS for existing infrastructure, and calculates the difference (the plan)
2. **Apply** - Terraform provisions or modifies infrastructure based on the plan
3. **State Management** - Terraform tracks infrastructure state to avoid re-creating existing resources

### Module Pattern

I use the module pattern to organize infrastructure code. Modules define one or more resources and are imported in `main.tf`. This approach makes the code reusable and maintainable.

### Key Terraform Concepts

**Data sources** - Query AWS for existing infrastructure and retrieve metadata such as ARNs, IDs, and security groups

**Variables** - Allow parameterization of infrastructure definitions. Locally, I assign variable values through `.tfvars` files. In GitHub Actions, I use environment variables and secrets

**Outputs** - Export values from Terraform for use in external scripts, such as shell commands in GitHub Actions

## Integrating Terraform with GitHub Actions

### Automation Workflow

I integrate Terraform with GitHub Actions to achieve full automation:

1. **Terraform Init** - Initializes the Terraform working directory and downloads required providers
2. **Terraform Plan** - Generates a plan file (`tfplan`) that describes infrastructure changes
3. **Terraform Apply** - Applies the plan to provision or update AWS infrastructure
4. **Wait for Deployment** - After applying changes, I use `aws ecs wait services-stable` to wait for the ECS service deployment to complete and health checks to pass before marking the workflow as successful

### Future Enhancements

Potential improvements include:

- Caching Terraform providers to speed up initialization
- Implementing Terraform workspaces for multiple environments
- Adding cost estimation tools like Infracost to the pipeline