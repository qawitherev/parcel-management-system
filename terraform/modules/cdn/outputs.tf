output "bucket_name" {
  description = "S3 bucket name (for aws s3 sync in CI/CD)"
  value       = aws_s3_bucket.frontend.id
}

output "bucket_arn" {
  description = "S3 bucket ARN"
  value       = aws_s3_bucket.frontend.arn
}

output "cloudfront_domain_name" {
  description = "CloudFront distribution domain name (for Route53 alias)"
  value       = aws_cloudfront_distribution.frontend.domain_name
}

output "cloudfront_zone_id" {
  description = "CloudFront hosted zone ID (always Z2FDTNDATAQYW2)"
  value       = local.cloudfront_zone_id
}

output "distribution_id" {
  description = "CloudFront distribution ID (for cache invalidation in CI/CD)"
  value       = aws_cloudfront_distribution.frontend.id
}
