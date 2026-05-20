# ============================================================
# SSM Parameters — Application Runtime Configuration
# ============================================================

locals {
  prefix = "/${var.environment}/backend"
}

# ============================================================
# SecureString — secrets
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
  description = "SendGrid API key"
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
  value       = "PLACEHOLDER"

  lifecycle {
    ignore_changes = [value]
  }

  tags = var.tags
}

resource "aws_ssm_parameter" "jwt_audience" {
  name        = "${local.prefix}/JWTSettings__Audience"
  description = "JWT audience"
  type        = "String"
  value       = "PLACEHOLDER"

  lifecycle {
    ignore_changes = [value]
  }

  tags = var.tags
}

resource "aws_ssm_parameter" "jwt_expiry" {
  name        = "${local.prefix}/JWTSettings__ExpirationMinutes"
  description = "JWT token expiry in minutes"
  type        = "String"
  value       = "PLACEHOLDER"

  lifecycle {
    ignore_changes = [value]
  }

  tags = var.tags
}

resource "aws_ssm_parameter" "email_username" {
  name        = "${local.prefix}/Notification__Email__Username"
  description = "SendGrid username (apikey)"
  type        = "String"
  value       = "PLACEHOLDER"

  lifecycle {
    ignore_changes = [value]
  }

  tags = var.tags
}

resource "aws_ssm_parameter" "email_smtp_host" {
  name        = "${local.prefix}/Notification__Email__SmtpHost"
  description = "SMTP server hostname"
  type        = "String"
  value       = "PLACEHOLDER"

  lifecycle {
    ignore_changes = [value]
  }

  tags = var.tags
}

resource "aws_ssm_parameter" "email_smtp_port" {
  name        = "${local.prefix}/Notification__Email__SmtpPort"
  description = "SMTP server port"
  type        = "String"
  value       = "PLACEHOLDER"

  lifecycle {
    ignore_changes = [value]
  }

  tags = var.tags
}

resource "aws_ssm_parameter" "email_from_address" {
  name        = "${local.prefix}/Notification__Email__FromAddress"
  description = "Sender email address"
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
