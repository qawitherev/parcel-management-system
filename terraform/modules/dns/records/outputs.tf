output "api_fqdn" {
  description = "API fully qualified domain name"
  value       = var.enable_compute ? aws_route53_record.api[0].fqdn : null
}

output "app_fqdn" {
  description = "App fully qualified domain name"
  value       = aws_route53_record.app.fqdn
}
