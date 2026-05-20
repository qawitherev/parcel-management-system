output "repository_name" {
  description = "ECR repository name"
  value       = aws_ecr_repository.backend.name
}

output "repository_url" {
  description = "ECR repository URL (used for docker push/pull)"
  value       = aws_ecr_repository.backend.repository_url
}

output "repository_arn" {
  description = "ECR repository ARN"
  value       = aws_ecr_repository.backend.arn
}
