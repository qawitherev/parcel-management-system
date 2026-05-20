variable "environment" {
  description = "Environment name (staging, production)"
  type        = string
}

variable "image_tag_mutability" {
  description = "Whether image tags can be overwritten"
  type        = string
  default     = "IMMUTABLE"
}

variable "scan_on_push" {
  description = "Run vulnerability scan on every image push"
  type        = bool
  default     = true
}

variable "max_images" {
  description = "Maximum number of tagged images to retain"
  type        = number
  default     = 10
}

variable "untagged_expiry_days" {
  description = "Days before untagged images are expired"
  type        = number
  default     = 7
}

variable "force_delete" {
  description = "Allow Terraform destroy even if repo has images"
  type        = bool
  default     = false
}

variable "tags" {
  description = "Tags applied to all resources"
  type        = map(string)
  default     = {}
}
