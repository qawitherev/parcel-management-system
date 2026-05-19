output "arn" {
  description = "ALB ARN"
  value       = aws_lb.this.arn
}

output "dns_name" {
  description = "ALB DNS name"
  value       = aws_lb.this.dns_name
}

output "zone_id" {
  description = "ALB canonical hosted zone ID (for Route53 alias records)"
  value       = aws_lb.this.zone_id
}

output "target_group_arn" {
  description = "Backend target group ARN — passed to ECS service"
  value       = aws_lb_target_group.backend.arn
}

output "listener_http_arn" {
  description = "HTTP listener ARN"
  value       = aws_lb_listener.http.arn
}

output "listener_https_arn" {
  description = "HTTPS listener ARN (null if certificate not configured)"
  value       = length(aws_lb_listener.https) > 0 ? aws_lb_listener.https[0].arn : null
}
