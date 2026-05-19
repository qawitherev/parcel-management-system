# ============================================================
# SSM Parameters — Application Runtime Configuration
# ============================================================
#
# HOW TO SET REAL VALUES (one-time, after terraform apply):
#
#   aws ssm put-parameter \
#     --name "/{env}/backend/ConnectionStrings__DefaultConnection" \
#     --value "Server=...;Database=...;User=...;Password=..." \
#     --type SecureString --overwrite
#
#   Or use AWS Console → Systems Manager → Parameter Store.
#
# Terraform will NOT overwrite manually-set values (ignore_changes).
# ============================================================

locals {
  prefix = "/${var.environment}/backend"
}

# ============================================================
# SecureString — secrets (set real values after first apply)
# ============================================================

resource "aws_ssm_parameter" "db_connection" {
  name        = "${local.prefix}/ConnectionStrings__DefaultConnection"
  description = "MySQL connection string"
  type        = "SecureString"
  value       = "PLACEHOLDER"

  lifecycle {
    ignore_changes = [value]
  }

  tags = var.tags
}

resource "aws_ssm_parameter" "jwt_secret" {
  name        = "${local.prefix}/JWTSettings__SecretKey"
  description = "JWT signing key"
  type        = "SecureString"
  value       = "PLACEHOLDER"

  lifecycle {
    ignore_changes = [value]
  }

  tags = var.tags
}

resource "aws_ssm_parameter" "email_password" {
  name        = "${local.prefix}/Notification__Email__Password"
  description = "SendGrid API key or SMTP password"
  type        = "SecureString"
  value       = "PLACEHOLDER"

  lifecycle {
    ignore_changes = [value]
  }

  tags = var.tags
}

resource "aws_ssm_parameter" "whatsapp_sid" {
  name        = "${local.prefix}/Notification__WhatsApp__AccountSid"
  description = "Twilio account SID"
  type        = "SecureString"
  value       = "PLACEHOLDER"

  lifecycle {
    ignore_changes = [value]
  }

  tags = var.tags
}

resource "aws_ssm_parameter" "whatsapp_token" {
  name        = "${local.prefix}/Notification__WhatsApp__AuthToken"
  description = "Twilio auth token"
  type        = "SecureString"
  value       = "PLACEHOLDER"

  lifecycle {
    ignore_changes = [value]
  }

  tags = var.tags
}

resource "aws_ssm_parameter" "redis_connection" {
  name        = "${local.prefix}/RedisSettings__ConnectionString"
  description = "Redis connection string"
  type        = "SecureString"
  value       = "PLACEHOLDER"

  lifecycle {
    ignore_changes = [value]
  }

  tags = var.tags
}

resource "aws_ssm_parameter" "admin_password" {
  name        = "${local.prefix}/Admin__Password"
  description = "Admin account password"
  type        = "SecureString"
  value       = "PLACEHOLDER"

  lifecycle {
    ignore_changes = [value]
  }

  tags = var.tags
}

# ============================================================
# String — non-sensitive config
# ============================================================

resource "aws_ssm_parameter" "jwt_issuer" {
  name        = "${local.prefix}/JWTSettings__Issuer"
  description = "JWT issuer"
  type        = "String"
  value       = "parcel-management-${var.environment}"

  tags = var.tags
}

resource "aws_ssm_parameter" "jwt_audience" {
  name        = "${local.prefix}/JWTSettings__Audience"
  description = "JWT audience"
  type        = "String"
  value       = "parcel-management-api"

  tags = var.tags
}

resource "aws_ssm_parameter" "jwt_expiry" {
  name        = "${local.prefix}/JWTSettings__ExpirationMinutes"
  description = "JWT token expiry in minutes"
  type        = "String"
  value       = "60"

  tags = var.tags
}

resource "aws_ssm_parameter" "email_username" {
  name        = "${local.prefix}/Notification__Email__Username"
  description = "SMTP username"
  type        = "String"
  value       = "PLACEHOLDER"

  lifecycle {
    ignore_changes = [value]
  }

  tags = var.tags
}

resource "aws_ssm_parameter" "whatsapp_from" {
  name        = "${local.prefix}/Notification__WhatsApp__FromNumber"
  description = "Twilio from phone number"
  type        = "String"
  value       = "PLACEHOLDER"

  lifecycle {
    ignore_changes = [value]
  }

  tags = var.tags
}

resource "aws_ssm_parameter" "admin_email" {
  name        = "${local.prefix}/Admin__Email"
  description = "Admin account email"
  type        = "String"
  value       = "PLACEHOLDER"

  lifecycle {
    ignore_changes = [value]
  }

  tags = var.tags
}
