terraform {
  required_version = ">=1.0"
  
  required_providers {
    aws = {
        source = "hashicorp/aws"
        version = "~>5.0"
    }
  }
}

provider "aws" {
  region = var.region
}

module "ecs" {
  source = "../../modules/ecs"
  cluster_name = var.cluster_name
  tags = var.tags
  task_definition_family = var.task_definition_family
  container_definitions = jsonencode(var.container_definitions)
  task_cpu = var.task_cpu
  task_memory = var.task_memory
  task_enable_fault_injection = var.task_enable_fault_injection
  task_execution_role_arn = var.task_execution_role_arn
  task_role_arn = var.task_role_arn
  ecs_service_name = var.ecs_service_name
  ecs_service_desired_count = var.ecs_service_desired_count
  ecs_service_subnets = var.ecs_service_subnets
  ecs_service_security_groups = var.ecs_service_security_groups
}