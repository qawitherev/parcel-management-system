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
  execution_role_arn = var.task_execution_role_arn
  task_role_arn = var.task_role_arn
  network_mode = "awsvpc" # we will always use this value 

  tags = var.tags


  container_definitions = templatefile("${path.module}/templates/container_definition.json", {
    github_sha         = var.github_sha
    ecr_repository_url = var.ecr_repository_url
    ssm_param_arns = jsonencode([
      for name in [
        "ConnectionStrings__DefaultConnection",
        "JWTSettings__SecretKey",
        "JWTSettings__Issuer",
        "JWTSettings__ExpirationMinutes",
        "JWTSettings__Audience",
        "RedisSettings__ConnectionString",
        "Admin__Email",
        "Admin__Password",
        "Notification__Email__Username",
        "Notification__Email__Password",
        "Notification__Email__SmtpHost",
        "Notification__Email__SmtpPort",
        "Notification__Email__FromAddress",
        "DbCACert",
        "AllowedOrigins",
      ] : {
        name      = name
        valueFrom = "arn:aws:ssm:${var.aws_region}:${var.aws_account_id}:parameter/${var.ssm_prefix}/${name}"
    }])
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
    assign_public_ip = var.assign_public_ip
  }

  dynamic "load_balancer" {
    for_each = var.alb_target_group_arn != null ? [1] : []
    content {
      target_group_arn = var.alb_target_group_arn
      container_name   = "backend"
      container_port   = 5163
    }
  }
}