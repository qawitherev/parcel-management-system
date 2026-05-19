output "parameter_arns" {
  description = "Map of SSM parameter ARNs"
  value = {
    db_connection     = aws_ssm_parameter.db_connection.arn
    jwt_secret        = aws_ssm_parameter.jwt_secret.arn
    email_password    = aws_ssm_parameter.email_password.arn
    whatsapp_sid      = aws_ssm_parameter.whatsapp_sid.arn
    whatsapp_token    = aws_ssm_parameter.whatsapp_token.arn
    redis_connection  = aws_ssm_parameter.redis_connection.arn
    admin_password    = aws_ssm_parameter.admin_password.arn
    jwt_issuer        = aws_ssm_parameter.jwt_issuer.arn
    jwt_audience      = aws_ssm_parameter.jwt_audience.arn
    jwt_expiry        = aws_ssm_parameter.jwt_expiry.arn
    email_username    = aws_ssm_parameter.email_username.arn
    whatsapp_from     = aws_ssm_parameter.whatsapp_from.arn
    admin_email       = aws_ssm_parameter.admin_email.arn
  }
}
