output "arn" {
  description = "ALB ARN"
  value       = var.enable_compute ? aws_lb.this[0].arn : null
}

output "dns_name" {
  description = "ALB DNS name"
  value       = var.enable_compute ? aws_lb.this[0].dns_name : null
}

output "zone_id" {
  description = "ALB canonical hosted zone ID (for Route53 alias records)"
  value       = var.enable_compute ? aws_lb.this[0].zone_id : null
}

output "target_group_arn" {
  description = "Backend target group ARN — passed to ECS service"
  value       = var.enable_compute ? aws_lb_target_group.backend[0].arn : null
}

output "listener_http_arn" {
  description = "HTTP listener ARN"
  value       = var.enable_compute ? aws_lb_listener.http[0].arn : null
}

output "listener_https_arn" {
  description = "HTTPS listener ARN"
  value       = var.enable_compute ? aws_lb_listener.https[0].arn : null
}
