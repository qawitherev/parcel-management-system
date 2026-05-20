output "alb_security_group_id" {
  description = "ALB security group ID"
  value       = aws_security_group.alb.id
}

output "ecs_security_group_id" {
  description = "ECS tasks security group ID"
  value       = aws_security_group.ecs_tasks.id
}

output "task_execution_role_arn" {
  description = "ECS task execution role ARN"
  value       = aws_iam_role.task_execution.arn
}

output "task_role_arn" {
  description = "ECS task role ARN"
  value       = aws_iam_role.task.arn
}

output "cicd_access_key_id" {
  description = "CI/CD deployment user access key ID"
  value       = aws_iam_access_key.cicd.id
}

output "cicd_access_key_secret" {
  description = "CI/CD deployment user access key secret"
  value       = aws_iam_access_key.cicd.secret
  sensitive   = true
}
