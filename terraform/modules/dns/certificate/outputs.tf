output "certificate_arn" {
  description = "ACM certificate ARN for the ALB HTTPS listener"
  value       = aws_acm_certificate_validation.this.certificate_arn
}

output "app_domain" {
  description = "App domain covered by the certificate"
  value       = local.app_domain
}

output "api_domain" {
  description = "API domain covered by the certificate"
  value       = local.api_domain
}
