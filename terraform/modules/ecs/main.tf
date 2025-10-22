resource "aws_ecs_cluster" "this" {
  name = var.cluster_name
  setting {
    name = "containerInsights"
    value = "enabled"
  }

  tags = var.tags
}

resource "aws_ecs_task_definition" "this" {
  family = var.task_definition_family
  requires_compatibilities = "FARGATE"
  cpu = var.task_cpu
  memory = var.task_memory
  enable_fault_injection = var.task_enable_fault_injection
  execution_role_arn = var.task_execution_role_arn
  task_role_arn = var.task_role_arn
  network_mode = "awsvpc" # we will always use this value 

  tags = var.tags


  container_definitions = var.container_definitions
}