variable "environment" {
  description = "Environment name (staging, production)"
  type        = string
}

variable "app_domain" {
  description = "Custom domain name for the frontend (e.g., staging.parcel-management.qawitherev.com)"
  type        = string
}

variable "zone_id" {
  description = "Route53 hosted zone ID for DNS validation records"
  type        = string
}

variable "price_class" {
  description = "CloudFront price class (edge location coverage)"
  type        = string
  default     = "PriceClass_200"
}

variable "tags" {
  description = "Tags applied to all resources"
  type        = map(string)
  default     = {}
}
