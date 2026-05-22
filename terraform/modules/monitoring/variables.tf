variable "environment" {
  description = "Environment name (staging, production)"
  type        = string
}

variable "region" {
  description = "AWS region"
  type        = string
}

variable "enable_compute" {
  description = "When false, destroy the dashboard (no metrics without ALB/ECS)"
  type        = bool
  default     = true
}

variable "alb_arn_suffix" {
  description = "ALB ARN suffix (e.g. app/name/hex)"
  type        = string
  default     = null
}

variable "target_group_arn_suffix" {
  description = "Target group ARN suffix"
  type        = string
  default     = null
}

variable "ecs_cluster_name" {
  description = "ECS cluster name"
  type        = string
}

variable "ecs_service_name" {
  description = "ECS service name"
  type        = string
  default     = null
}

variable "cloudfront_distribution_id" {
  description = "CloudFront distribution ID"
  type        = string
}

variable "tags" {
  description = "Tags applied to the dashboard"
  type        = map(string)
}
