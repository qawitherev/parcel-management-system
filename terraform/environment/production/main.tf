terraform {
  required_version = ">=1.0"
  backend "s3" {
    bucket       = "parcel-management-system"
    key          = "terraform/production/terraform.tfstate"
    region       = "ap-southeast-1"
    use_lockfile = true
  }

  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = "~>5.0"
    }
  }
}

provider "aws" {
  region = "ap-southeast-1"
}

module "ecs" {
  source                      = "../../modules/ecs"
  cluster_name                = var.cluster_name
  tags                        = var.tags
  task_definition_family      = var.task_definition_family
  github_sha                  = var.github_sha
  task_cpu                    = var.task_cpu
  task_memory                 = var.task_memory
  task_enable_fault_injection = var.task_enable_fault_injection
  task_execution_role_arn     = data.aws_iam_role.ecsTaskExecutionRole.arn
  task_role_arn               = data.aws_iam_role.ecsTaskExecutionRole.arn
  ecs_service_name            = var.ecs_service_name
  ecs_service_desired_count   = var.ecs_service_desired_count
  ecs_service_subnets         = [data.aws_subnets.default_subnet.ids[0]]
  ecs_service_security_groups = [data.aws_security_group.default_sg.id]
}