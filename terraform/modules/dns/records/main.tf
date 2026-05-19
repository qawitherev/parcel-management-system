locals {
  prefix     = var.environment == "production" ? "" : "${var.environment}."
  app_domain = "${local.prefix}parcel-management.${var.root_domain}"
  api_domain = "api.${local.prefix}parcel-management.${var.root_domain}"
}

resource "aws_route53_record" "api" {
  zone_id = var.zone_id
  name    = local.api_domain
  type    = "A"

  alias {
    name                   = var.alb_dns_name
    zone_id                = var.alb_zone_id
    evaluate_target_health = true
  }
}

resource "aws_route53_record" "app" {
  zone_id = var.zone_id
  name    = local.app_domain
  type    = "A"

  alias {
    name                   = var.cloudfront_domain_name
    zone_id                = var.cloudfront_zone_id
    evaluate_target_health = false
  }
}
