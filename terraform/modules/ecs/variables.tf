variable "cluster_name" {
  description = "cluster name "
  type = string
}

variable "tags" {
  description = "tags in collection of map<string>"
  type = map(string)
}

variable "task_definition_family" {
  description = "task definition family"
  type = string
}

variable "task_cpu" {
  description = "CPU unit for task"
  type = string
  default = "256"
}

variable "task_memory" {
  description = "amount of memory to be given to task"
  type = string
  default = "512"
}

variable "task_enable_fault_injection" {
  description = "flag to enable fault injection"
  type = bool
  default = false
}

variable "task_execution_role_arn" {
  description = "arn for execution role"
  type = string
}

variable "task_role_arn" {
  description = "arn for task role"
  type = string
}

variable "ecs_service_name" {
  description = "service name"
  type = string
}

variable "ecs_service_desired_count" {
  description = "desired running service instance count"
}

variable "ecs_service_subnets" {
  description = "service subnets"
  type = set(string)
}

variable "ecs_service_security_groups" {
  description = "service sgs"
  type = set(string)
}

variable "github_sha" {
  description = "sha value from git commit. Enforce uniqueness"
  type = string
}