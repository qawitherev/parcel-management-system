variable "environment" {
  description = "Environment name (staging, production)"
  type        = string
}

variable "cluster_name" {
  description = "ECS cluster name"
  type        = string
}

variable "tags" {
  description = "Tags applied to all resources (must include Environment key)"
  type        = map(string)
}

variable "task_definition_family" {
  description = "ECS task definition family"
  type        = string
}

variable "github_sha" {
  description = "Git commit SHA for image tagging (set by CI/CD)"
  type        = string
}

variable "task_cpu" {
  description = "CPU units for the ECS task"
  type        = string
}

variable "task_memory" {
  description = "Memory (MB) for the ECS task"
  type        = string
}

variable "ecs_service_name" {
  description = "ECS service name"
  type        = string
}

variable "ecs_service_desired_count" {
  description = "Desired running task count"
  type        = number
}

variable "region" {
  description = "AWS region"
  type        = string
  default     = "ap-southeast-1"
}

variable "vpc_cidr" {
  description = "CIDR block for the VPC"
  type        = string
}

variable "public_subnet_cidrs" {
  description = "CIDR blocks for public subnets (2 AZs)"
  type        = list(string)
}

variable "private_subnet_cidrs" {
  description = "CIDR blocks for private subnets (2 AZs)"
  type        = list(string)
}

variable "availability_zones" {
  description = "Availability zones"
  type        = list(string)
}

variable "enable_compute" {
  description = "When false, destroy ALB, NAT Gateway, ECS service, and DNS API record to save cost (~$52/mo)"
  type        = bool
  default     = true
}
