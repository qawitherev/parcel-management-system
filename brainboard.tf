# ============================================================
# Brainboard — Parcel Management System Architecture
# ============================================================
# Only core infrastructure for a clean diagram.
# Not for apply.
# ============================================================

terraform {
  required_providers {
    aws = { source = "hashicorp/aws" }
  }
}

provider "aws" {
  region = "ap-southeast-1"
}

provider "aws" {
  alias  = "us_east_1"
  region = "us-east-1"
}

# ============================================================
# VPC + NETWORKING
# ============================================================

resource "aws_vpc" "main" {
  cidr_block           = "10.0.0.0/16"
  enable_dns_support   = true
  enable_dns_hostnames = true

  tags = { Name = "parcel-management-production" }
}

resource "aws_internet_gateway" "main" {
  vpc_id = aws_vpc.main.id
  tags   = { Name = "parcel-management-production" }
}

resource "aws_eip" "nat" {
  domain = "vpc"
}

resource "aws_nat_gateway" "main" {
  allocation_id = aws_eip.nat.id
  subnet_id     = aws_subnet.public[0].id

  depends_on = [aws_internet_gateway.main]
  tags       = { Name = "parcel-management-production" }
}

resource "aws_subnet" "public" {
  count                   = 2
  vpc_id                  = aws_vpc.main.id
  cidr_block              = ["10.0.1.0/24", "10.0.2.0/24"][count.index]
  availability_zone       = ["ap-southeast-1a", "ap-southeast-1b"][count.index]
  map_public_ip_on_launch = true

  tags = { Name = "public-${count.index + 1}" }
}

resource "aws_subnet" "private" {
  count             = 2
  vpc_id            = aws_vpc.main.id
  cidr_block        = ["10.0.3.0/24", "10.0.4.0/24"][count.index]
  availability_zone = ["ap-southeast-1a", "ap-southeast-1b"][count.index]

  tags = { Name = "private-${count.index + 1}" }
}

resource "aws_route_table" "public" {
  vpc_id = aws_vpc.main.id

  route {
    cidr_block = "0.0.0.0/0"
    gateway_id = aws_internet_gateway.main.id
  }
}

resource "aws_route_table_association" "public" {
  count          = 2
  subnet_id      = aws_subnet.public[count.index].id
  route_table_id = aws_route_table.public.id
}

resource "aws_route_table" "private" {
  vpc_id = aws_vpc.main.id

  route {
    cidr_block     = "0.0.0.0/0"
    nat_gateway_id = aws_nat_gateway.main.id
  }
}

resource "aws_route_table_association" "private" {
  count          = 2
  subnet_id      = aws_subnet.private[count.index].id
  route_table_id = aws_route_table.private.id
}

# ============================================================
# ACM CERTIFICATES (SSL)
# ============================================================

resource "aws_acm_certificate" "alb" {
  domain_name               = "parcel-management.qawitherev.com"
  subject_alternative_names = ["api-parcel-management.qawitherev.com"]
  validation_method         = "DNS"

  lifecycle { create_before_destroy = true }
  tags      = { Name = "alb-certificate" }
}

resource "aws_acm_certificate" "cdn" {
  provider          = aws.us_east_1
  domain_name       = "parcel-management.qawitherev.com"
  validation_method = "DNS"

  lifecycle { create_before_destroy = true }
  tags      = { Name = "cdn-certificate" }
}

# ============================================================
# ALB (public, in public subnets)
# ============================================================

resource "aws_lb" "main" {
  name               = "parcel-management-production"
  internal           = false
  load_balancer_type = "application"
  security_groups    = [aws_security_group.alb.id]
  subnets            = aws_subnet.public[*].id

  tags = { Name = "parcel-management-production" }
}

resource "aws_lb_target_group" "backend" {
  name        = "pm-production-backend"
  port        = 5163
  protocol    = "HTTP"
  vpc_id      = aws_vpc.main.id
  target_type = "ip"

  health_check {
    path                = "/health"
    protocol            = "HTTP"
    matcher             = "200"
    interval            = 30
    timeout             = 5
    healthy_threshold   = 2
    unhealthy_threshold = 3
  }
}

resource "aws_lb_listener" "https" {
  load_balancer_arn = aws_lb.main.arn
  port              = "443"
  protocol          = "HTTPS"
  certificate_arn   = aws_acm_certificate.alb.arn

  default_action {
    type             = "forward"
    target_group_arn = aws_lb_target_group.backend.arn
  }
}

# ============================================================
# SECURITY GROUPS
# ============================================================

resource "aws_security_group" "alb" {
  name        = "parcel-management-production-alb"
  description = "Allow HTTPS from internet"
  vpc_id      = aws_vpc.main.id

  ingress {
    from_port   = 443
    to_port     = 443
    protocol    = "tcp"
    cidr_blocks = ["0.0.0.0/0"]
  }

  egress {
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
  }
}

resource "aws_security_group" "ecs" {
  name        = "parcel-management-production-ecs"
  description = "Accept traffic only from ALB"
  vpc_id      = aws_vpc.main.id

  ingress {
    from_port       = 5163
    to_port         = 5163
    protocol        = "tcp"
    security_groups = [aws_security_group.alb.id]
  }

  egress {
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
  }
}

# ============================================================
# ECR (container images)
# ============================================================

resource "aws_ecr_repository" "backend" {
  name                 = "parcel-management-production-backend"
  image_tag_mutability = "MUTABLE"
  force_delete         = true

  image_scanning_configuration {
    scan_on_push = true
  }

  tags = { Name = "backend-ecr" }
}

# ============================================================
# ECS FARGATE (in private subnets)
# ============================================================

resource "aws_ecs_cluster" "main" {
  name = "parcel-management-production"

  setting {
    name  = "containerInsights"
    value = "enabled"
  }

  tags = { Name = "ecs-cluster" }
}

resource "aws_ecs_task_definition" "backend" {
  family                   = "parcel-management-production-backend"
  requires_compatibilities = ["FARGATE"]
  cpu                      = "512"
  memory                   = "1024"
  network_mode             = "awsvpc"

  container_definitions = jsonencode([{
    name  = "backend"
    image = "${aws_ecr_repository.backend.repository_url}:latest"
    portMappings = [{
      containerPort = 5163
      protocol      = "tcp"
    }]
  }])

  tags = { Name = "backend-task-def" }
}

resource "aws_ecs_service" "backend" {
  name            = "parcel-management-production-backend"
  task_definition = aws_ecs_task_definition.backend.arn
  cluster         = aws_ecs_cluster.main.arn
  desired_count   = 1
  launch_type     = "FARGATE"

  network_configuration {
    subnets          = aws_subnet.private[*].id
    security_groups  = [aws_security_group.ecs.id]
    assign_public_ip = false
  }

  load_balancer {
    target_group_arn = aws_lb_target_group.backend.arn
    container_name   = "backend"
    container_port   = 5163
  }

  tags = { Name = "backend-service" }
}

# ============================================================
# S3 + CLOUDFRONT (frontend hosting)
# ============================================================

resource "aws_s3_bucket" "frontend" {
  bucket        = "parcel-management-production-frontend"
  force_destroy = true

  tags = { Name = "frontend-bucket" }
}

resource "aws_cloudfront_origin_access_control" "frontend" {
  name                              = "parcel-management-production-frontend"
  origin_access_control_origin_type = "s3"
  signing_behavior                  = "always"
  signing_protocol                  = "sigv4"
}

resource "aws_cloudfront_distribution" "frontend" {
  enabled             = true
  default_root_object = "index.html"
  price_class         = "PriceClass_100"
  aliases             = ["parcel-management.qawitherev.com"]

  origin {
    domain_name              = aws_s3_bucket.frontend.bucket_regional_domain_name
    origin_id                = aws_s3_bucket.frontend.id
    origin_access_control_id = aws_cloudfront_origin_access_control.frontend.id
  }

  default_cache_behavior {
    allowed_methods        = ["GET", "HEAD", "OPTIONS"]
    cached_methods         = ["GET", "HEAD"]
    target_origin_id       = aws_s3_bucket.frontend.id
    viewer_protocol_policy = "redirect-to-https"
    compress               = true

    forwarded_values {
      query_string = false
      cookies { forward = "none" }
    }

    min_ttl     = 0
    default_ttl = 3600
    max_ttl     = 86400
  }

  custom_error_response {
    error_code         = 404
    response_code      = 200
    response_page_path = "/index.html"
  }

  viewer_certificate {
    acm_certificate_arn      = aws_acm_certificate.cdn.arn
    ssl_support_method       = "sni-only"
    minimum_protocol_version = "TLSv1.2_2021"
  }

  restrictions {
    geo_restriction {
      restriction_type = "none"
    }
  }

  tags = { Name = "cloudfront-distribution" }
}

# ============================================================
# ROUTE53 DNS
# ============================================================

resource "aws_route53_zone" "root" {
  name = "qawitherev.com"
}

resource "aws_route53_record" "api" {
  zone_id = aws_route53_zone.root.zone_id
  name    = "api-parcel-management.qawitherev.com"
  type    = "A"

  alias {
    name                   = aws_lb.main.dns_name
    zone_id                = aws_lb.main.zone_id
    evaluate_target_health = true
  }
}

resource "aws_route53_record" "app" {
  zone_id = aws_route53_zone.root.zone_id
  name    = "parcel-management.qawitherev.com"
  type    = "A"

  alias {
    name                   = aws_cloudfront_distribution.frontend.domain_name
    zone_id                = aws_cloudfront_distribution.frontend.hosted_zone_id
    evaluate_target_health = false
  }
}
