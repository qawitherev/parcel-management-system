output "parameter_arns" {
  description = "Map of SSM parameter ARNs"
  value = {
    db_connection     = aws_ssm_parameter.db_connection.arn
    jwt_secret        = aws_ssm_parameter.jwt_secret.arn
    email_password    = aws_ssm_parameter.email_password.arn
    redis_connection  = aws_ssm_parameter.redis_connection.arn
    admin_password    = aws_ssm_parameter.admin_password.arn
    jwt_issuer        = aws_ssm_parameter.jwt_issuer.arn
    jwt_audience      = aws_ssm_parameter.jwt_audience.arn
    jwt_expiry        = aws_ssm_parameter.jwt_expiry.arn
    email_username    = aws_ssm_parameter.email_username.arn
    email_smtp_host   = aws_ssm_parameter.email_smtp_host.arn
    email_smtp_port   = aws_ssm_parameter.email_smtp_port.arn
    email_from_address = aws_ssm_parameter.email_from_address.arn
    admin_email       = aws_ssm_parameter.admin_email.arn
    db_ca_cert        = aws_ssm_parameter.db_ca_cert.arn
  }
}
