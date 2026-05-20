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

variable "tags" {
  description = "Tags applied to all resources"
  type        = map(string)
  default     = {}
}
