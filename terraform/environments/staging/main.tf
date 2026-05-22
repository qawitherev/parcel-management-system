terraform {
  required_version = ">= 1.6"
  backend "s3" {
    bucket       = "parcel-management-system"
    key          = "terraform/staging/terraform.tfstate"
    region       = "ap-southeast-1"
  }

  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = "~>5.0"
    }
  }
}

provider "aws" {
  region = var.region
}

# CloudFront requires certificates in us-east-1
provider "aws" {
  alias  = "us_east_1"
  region = "us-east-1"
}

data "aws_caller_identity" "current" {}

# Hosted zone created once by production. Staging reads it.
data "aws_route53_zone" "root" {
  name = "qawitherev.com"
}

module "oidc" {
  source               = "../../modules/oidc"
  github_repo          = "qawitherev/parcel-management-system"
  environment          = var.environment
  allowed_branch       = "staging"
  create_oidc_provider = false
  tags                 = var.tags
}

module "networking" {
  source               = "../../modules/networking"
  environment          = var.environment
  vpc_cidr             = var.vpc_cidr
  public_subnet_cidrs  = var.public_subnet_cidrs
  private_subnet_cidrs = var.private_subnet_cidrs
  availability_zones   = var.availability_zones
  enable_compute       = var.enable_compute
  tags                 = var.tags
}

module "security" {
  source          = "../../modules/security"
  environment     = var.environment
  vpc_id          = module.networking.vpc_id
  aws_account_id  = data.aws_caller_identity.current.account_id
  tags            = var.tags
}

module "ecr" {
  source      = "../../modules/ecr"
  environment = var.environment
  tags        = var.tags
}

module "ssm" {
  source      = "../../modules/ssm"
  environment = var.environment
  tags        = var.tags
}

module "dns_certificate" {
  source      = "../../modules/dns/certificate"
  environment = var.environment
  zone_id     = data.aws_route53_zone.root.zone_id
  tags        = var.tags
}

module "cdn" {
  source      = "../../modules/cdn"
  providers   = {
    aws           = aws
    aws.us_east_1 = aws.us_east_1
  }
  environment = var.environment
  app_domain  = module.dns_certificate.app_domain
  zone_id     = data.aws_route53_zone.root.zone_id
  tags        = var.tags
}

module "alb" {
  source                  = "../../modules/alb"
  environment             = var.environment
  vpc_id                  = module.networking.vpc_id
  public_subnet_ids       = module.networking.public_subnet_ids
  alb_security_group_id   = module.security.alb_security_group_id
  certificate_arn         = module.dns_certificate.certificate_arn
  enable_compute          = var.enable_compute
  tags                    = var.tags
}

module "dns_records" {
  source                   = "../../modules/dns/records"
  environment              = var.environment
  zone_id                  = data.aws_route53_zone.root.zone_id
  alb_dns_name             = module.alb.dns_name
  alb_zone_id              = module.alb.zone_id
  cloudfront_domain_name   = module.cdn.cloudfront_domain_name
  cloudfront_zone_id       = module.cdn.cloudfront_zone_id
  enable_compute           = var.enable_compute
  tags                     = var.tags
}

module "ecs" {
  source                      = "../../modules/ecs"
  cluster_name                = var.cluster_name
  tags                        = var.tags
  task_definition_family      = var.task_definition_family
  github_sha                  = var.github_sha
  task_cpu                    = var.task_cpu
  task_memory                 = var.task_memory
  task_execution_role_arn     = module.security.task_execution_role_arn
  task_role_arn               = module.security.task_role_arn
  ecs_service_name            = var.ecs_service_name
  ecs_service_desired_count   = var.ecs_service_desired_count
  ecs_service_subnets         = module.networking.private_subnet_ids
  ecs_service_security_groups = [module.security.ecs_security_group_id]
  assign_public_ip            = false
  ecr_repository_url          = module.ecr.repository_url
  alb_target_group_arn        = module.alb.target_group_arn
  ssm_prefix                  = "${var.environment}/backend"
  aws_region                  = var.region
  aws_account_id              = data.aws_caller_identity.current.account_id
  enable_compute              = var.enable_compute
}

module "monitoring" {
  source                    = "../../modules/monitoring"
  environment               = var.environment
  region                    = var.region
  alb_arn_suffix            = var.enable_compute ? element(split(":loadbalancer/", module.alb.arn), 1) : null
  target_group_arn_suffix   = var.enable_compute ? element(split(":targetgroup/", module.alb.target_group_arn), 1) : null
  ecs_cluster_name          = module.ecs.cluster_name
  ecs_service_name          = module.ecs.service_name
  cloudfront_distribution_id = module.cdn.distribution_id
  enable_compute            = var.enable_compute
  tags                      = var.tags
}
