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
  requires_compatibilities = ["FARGATE"]
  cpu = var.task_cpu
  memory = var.task_memory
  enable_fault_injection = var.task_enable_fault_injection
  execution_role_arn = var.task_execution_role_arn
  task_role_arn = var.task_role_arn
  network_mode = "awsvpc" # we will always use this value 

  tags = var.tags


  container_definitions = templatefile("${path.module}/templates/container_definition.json", {
    github_sha = var.github_sha
  })
}

resource "aws_ecs_service" "this" {
  name = var.ecs_service_name
  task_definition = aws_ecs_task_definition.this.arn
  cluster = aws_ecs_cluster.this.arn
  desired_count = var.ecs_service_desired_count
  force_new_deployment = true
  launch_type = "FARGATE"

  network_configuration {
    subnets = var.ecs_service_subnets
    security_groups = var.ecs_service_security_groups
    assign_public_ip = true
  }

}