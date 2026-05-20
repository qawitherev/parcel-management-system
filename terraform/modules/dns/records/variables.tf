variable "environment" {
  description = "Environment name (staging, production)"
  type        = string
}

variable "root_domain" {
  description = "Root domain name (e.g., qawitherev.com)"
  type        = string
  default     = "qawitherev.com"
}

variable "zone_id" {
  description = "Route53 hosted zone ID for the root domain"
  type        = string
}

variable "alb_dns_name" {
  description = "ALB DNS name from the ALB module"
  type        = string
}

variable "alb_zone_id" {
  description = "ALB canonical hosted zone ID (for Route53 alias)"
  type        = string
}

variable "cloudfront_domain_name" {
  description = "CloudFront distribution domain name (from CDN module, Phase 8)"
  type        = string
  default     = null
}

variable "cloudfront_zone_id" {
  description = "CloudFront hosted zone ID (always Z2FDTNDATAQYW2)"
  type        = string
  default     = null
}

variable "tags" {
  description = "Tags applied to all resources"
  type        = map(string)
  default     = {}
}
