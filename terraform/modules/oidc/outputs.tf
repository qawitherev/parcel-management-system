output "role_arn" {
  description = "ARN of the GitHub Actions IAM role (use in workflow's role-to-assume)"
  value       = aws_iam_role.github_actions.arn
}

output "role_name" {
  description = "Name of the GitHub Actions IAM role"
  value       = aws_iam_role.github_actions.name
}
