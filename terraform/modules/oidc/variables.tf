variable "github_repo" {
  description = "GitHub repository in owner/repo format"
  type        = string
}

variable "environment" {
  description = "Environment name (staging, production)"
  type        = string
}

variable "allowed_branch" {
  description = "Git branch allowed to assume this role"
  type        = string
}

variable "create_oidc_provider" {
  description = "Whether to create the OIDC provider. Set true in only one environment."
  type        = bool
  default     = false
}

variable "tags" {
  description = "Tags applied to all resources"
  type        = map(string)
  default     = {}
}
