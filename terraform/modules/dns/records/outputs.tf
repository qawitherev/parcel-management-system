output "api_fqdn" {
  description = "API fully qualified domain name"
  value       = aws_route53_record.api.fqdn
}

output "app_fqdn" {
  description = "App fully qualified domain name"
  value       = aws_route53_record.app.fqdn
}
