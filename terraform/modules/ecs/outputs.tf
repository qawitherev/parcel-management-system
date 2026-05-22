output "cluster_name" {
  description = "ECS cluster name"
  value       = aws_ecs_cluster.this.name
}

output "cluster_arn" {
  description = "ECS cluster ARN"
  value       = aws_ecs_cluster.this.arn
}

output "service_name" {
  description = "ECS service name"
  value       = var.enable_compute ? aws_ecs_service.this[0].name : null
}

output "service_arn" {
  description = "ECS service ARN"
  value       = var.enable_compute ? aws_ecs_service.this[0].id : null
}

output "task_definition_arn" {
  description = "Task definition ARN"
  value       = aws_ecs_task_definition.this.arn
}
