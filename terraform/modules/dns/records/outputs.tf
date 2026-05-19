output "api_fqdn" {
  description = "API fully qualified domain name"
  value       = aws_route53_record.api.fqdn
}

output "app_fqdn" {
  description = "App fully qualified domain name"
  value       = length(aws_route53_record.app) > 0 ? aws_route53_record.app[0].fqdn : null
}
