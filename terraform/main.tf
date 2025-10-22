# terraform block 
terraform {
  required_version = ">=1.0"

  required_providers {
    aws = {
        source = "hashicorp/aws"
        version = "~>5.0"
    }
  }
}


# provider block 
provider "aws" {
  region = "ap-southeast-1"
  # access key and access secret key will use whats we have configured during aws configure (aws cli)
}


# data block
data "aws_vpc" "default" {
    default = true
}

data "aws_subnets" "default" {
    filter {
        name = "vpc-id"
        values = [data.aws_vpc.default.id]
    }
}

data "aws_security_groups" "security_groups" {
  filter {
    name = "vpc-id"
    values = [data.aws_vpc.default.id]
  }
}

resource "aws_ecs_cluster" "main_cluster" {
    name     = "parcel-management-cluster-qawitherev"
    tags     = {}
    tags_all = {}

    configuration {
        execute_command_configuration {
            kms_key_id = null
            logging    = "DEFAULT"
        }
    }

    setting {
        name  = "containerInsights"
        value = "disabled"
    }
}

resource "aws_cloudwatch_log_group" "ecs_cloudwatch_log_group" {
    kms_key_id        = null
    log_group_class   = "STANDARD"
    name              = "/ecs/parcel-management-system"
    name_prefix       = null
    retention_in_days = 0
    skip_destroy      = false
    tags              = {}
}

resource "aws_ecs_task_definition" "task_definition" {
    container_definitions    = jsonencode(
        [
            {
                environment      = []
                environmentFiles = [
                    {
                        type  = "s3"
                        value = "arn:aws:s3:::parcel-management-system/parcel-management-prod.env"
                    },
                ]
                essential        = true
                image            = "qawitherev/backend:latest"
                logConfiguration = {
                    logDriver     = "awslogs"
                    options       = {
                        awslogs-create-group  = "true"
                        awslogs-group         = "/ecs/parcel-management-system"
                        awslogs-region        = "ap-southeast-1"
                        awslogs-stream-prefix = "ecs"
                    }
                    secretOptions = []
                }
                mountPoints      = []
                name             = "backend"
                portMappings     = [
                    {
                        appProtocol   = "http"
                        containerPort = 5163
                        hostPort      = 5163
                        name          = "backend-port"
                        protocol      = "tcp"
                    },
                ]
                systemControls   = []
                ulimits          = []
                volumesFrom      = []
            },
            {
                environment      = [
                    {
                        name  = "BACKEND_HOST"
                        value = "localhost"
                    },
                    {
                        name  = "BACKEND_PORT"
                        value = "5163"
                    },
                ]
                environmentFiles = []
                essential        = true
                image            = "qawitherev/frontend:latest"
                logConfiguration = {
                    logDriver     = "awslogs"
                    options       = {
                        awslogs-create-group  = "true"
                        awslogs-group         = "/ecs/parcel-management-system"
                        awslogs-region        = "ap-southeast-1"
                        awslogs-stream-prefix = "ecs"
                    }
                    secretOptions = []
                }
                mountPoints      = []
                name             = "frontend"
                portMappings     = [
                    {
                        containerPort = 80
                        hostPort      = 80
                        name          = "frontend-port"
                        protocol      = "tcp"
                    },
                ]
                systemControls   = []
                volumesFrom      = []
            },
        ]
    )
    cpu                      = "256"
    enable_fault_injection   = true
    execution_role_arn       = "arn:aws:iam::012794607222:role/ecsTaskExecutionRole"
    family                   = "parcel-management-system"
    ipc_mode                 = null
    memory                   = "512"
    network_mode             = "awsvpc"
    pid_mode                 = null
    requires_compatibilities = [
        "FARGATE",
    ]
    tags                     = {}
    task_role_arn            = "arn:aws:iam::012794607222:role/ecsTaskExecutionRole"
    track_latest             = false

    runtime_platform {
        cpu_architecture        = "X86_64"
        operating_system_family = "LINUX"
    }
}

resource "aws_ecs_service" "main_service" {
  name = "parcel-management-service"
  cluster = aws_ecs_cluster.main_cluster.id
  task_definition = aws_ecs_task_definition.task_definition.id
  desired_count = 1
  launch_type = "FARGATE"
  deployment_minimum_healthy_percent = 0
  deployment_maximum_percent = 100

  network_configuration {
    subnets = [data.aws_subnets.default.ids[0]]
    security_groups  = [data.aws_security_groups.security_groups.ids[0]]
    assign_public_ip = true
  }

  deployment_circuit_breaker {
    enable = true
    rollback = true
  }

  lifecycle {
    ignore_changes = [ desired_count ]
  }


}

