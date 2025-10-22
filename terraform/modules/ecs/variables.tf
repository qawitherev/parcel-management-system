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

variable "container_definitions" {
  description = "Container defintion in JSON"
  type = any
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