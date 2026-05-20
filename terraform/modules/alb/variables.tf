variable "environment" {
  description = "Environment name (staging, production)"
  type        = string
}

variable "vpc_id" {
  description = "VPC ID where the ALB is deployed"
  type        = string
}

variable "public_subnet_ids" {
  description = "Public subnet IDs for ALB placement"
  type        = list(string)
}

variable "alb_security_group_id" {
  description = "Security group ID for the ALB"
  type        = string
}

variable "backend_port" {
  description = "Backend container port"
  type        = number
  default     = 5163
}

variable "health_check_path" {
  description = "Path for health checks"
  type        = string
  default     = "/health"
}

variable "certificate_arn" {
  description = "ACM certificate ARN for HTTPS (null = HTTPS disabled until Phase 7)"
  type        = string
  default     = null
}

variable "tags" {
  description = "Tags applied to all resources"
  type        = map(string)
  default     = {}
}
