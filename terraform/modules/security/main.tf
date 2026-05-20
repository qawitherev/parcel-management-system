# ============================================================
# Security Groups
# ============================================================

resource "aws_security_group" "alb" {
  name        = "parcel-management-${var.environment}-alb"
  description = "ALB security group - allows HTTPS from internet"
  vpc_id      = var.vpc_id

  ingress {
    description = "HTTPS from internet"
    from_port   = 443
    to_port     = 443
    protocol    = "tcp"
    cidr_blocks = ["0.0.0.0/0"]
  }

  ingress {
    description = "HTTP for redirect to HTTPS"
    from_port   = 80
    to_port     = 80
    protocol    = "tcp"
    cidr_blocks = ["0.0.0.0/0"]
  }

  egress {
    description = "All outbound (response to clients + forwarding to ECS)"
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
  }

  tags = merge(var.tags, {
    Name = "parcel-management-${var.environment}-alb"
  })
}

resource "aws_security_group" "ecs_tasks" {
  name        = "parcel-management-${var.environment}-ecs-tasks"
  description = "ECS tasks security group - accepts traffic only from ALB"
  vpc_id      = var.vpc_id

  ingress {
    description     = "Backend port from ALB only"
    from_port       = var.ecs_task_port
    to_port         = var.ecs_task_port
    protocol        = "tcp"
    security_groups = [aws_security_group.alb.id]
  }

  egress {
    description = "All outbound (ECR pull, external APIs)"
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
  }

  tags = merge(var.tags, {
    Name = "parcel-management-${var.environment}-ecs-tasks"
  })
}

# ============================================================
# IAM — ECS Task Execution Role
# ============================================================
# Used by the ECS agent BEFORE the container starts.
# Pulls images from ECR, writes to CloudWatch, reads SSM secrets.

resource "aws_iam_role" "task_execution" {
  name = "parcel-management-${var.environment}-task-execution"

  assume_role_policy = jsonencode({
    Version = "2012-10-17"
    Statement = [{
      Effect = "Allow"
      Principal = {
        Service = "ecs-tasks.amazonaws.com"
      }
      Action = "sts:AssumeRole"
    }]
  })

  tags = var.tags
}

resource "aws_iam_role_policy_attachment" "task_execution_ecs" {
  role       = aws_iam_role.task_execution.name
  policy_arn = "arn:aws:iam::aws:policy/service-role/AmazonECSTaskExecutionRolePolicy"
}

resource "aws_iam_role_policy" "task_execution_ssm" {
  name = "parcel-management-${var.environment}-ssm-read"
  role = aws_iam_role.task_execution.name

  policy = jsonencode({
    Version = "2012-10-17"
    Statement = [{
      Effect = "Allow"
      Action = [
        "ssm:GetParameters",
        "ssm:GetParameter"
      ]
      Resource = [
        "arn:aws:ssm:${var.region}:${var.aws_account_id}:parameter/${var.environment}/backend/*"
      ]
    }]
  })
}

# ============================================================
# IAM — ECS Task Role
# ============================================================
# Used by the app code INSIDE the running container.
# App-level AWS calls (S3, SES, SQS, etc.) go through this.

resource "aws_iam_role" "task" {
  name = "parcel-management-${var.environment}-task"

  assume_role_policy = jsonencode({
    Version = "2012-10-17"
    Statement = [{
      Effect = "Allow"
      Principal = {
        Service = "ecs-tasks.amazonaws.com"
      }
      Action = "sts:AssumeRole"
    }]
  })

  tags = var.tags
}

# Task role starts with no inline policies. Add S3, SES, SQS permissions
# here as the app needs them. Template:
#
# resource "aws_iam_role_policy" "task_ses" {
#   name = "parcel-management-${var.environment}-task-ses"
#   role = aws_iam_role.task.name
#   policy = jsonencode({
#     Version = "2012-10-17"
#     Statement = [{
#       Effect   = "Allow"
#       Action   = ["ses:SendEmail", "ses:SendRawEmail"]
#       Resource = "*"
#     }]
#   })
# }

# ============================================================
# IAM — CI/CD Deployment User
# ============================================================
# Used by GitHub Actions pipelines.
# ECR push, ECS deploy, S3 frontend upload, CloudFront invalidate.

resource "aws_iam_user" "cicd" {
  name = "parcel-management-${var.environment}-cicd"

  tags = var.tags
}

resource "aws_iam_user_policy" "cicd" {
  name = "parcel-management-${var.environment}-cicd-deploy"
  user = aws_iam_user.cicd.name

  policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Effect = "Allow"
        Action = [
          "ecr:GetAuthorizationToken",
          "ecr:BatchCheckLayerAvailability",
          "ecr:GetDownloadUrlForLayer",
          "ecr:GetRepositoryPolicy",
          "ecr:DescribeRepositories",
          "ecr:ListImages",
          "ecr:DescribeImages",
          "ecr:BatchGetImage",
          "ecr:InitiateLayerUpload",
          "ecr:UploadLayerPart",
          "ecr:CompleteLayerUpload",
          "ecr:PutImage"
        ]
        Resource = "*"
      },
      {
        Effect = "Allow"
        Action = [
          "ecs:UpdateService",
          "ecs:DescribeServices",
          "ecs:DescribeTaskDefinition",
          "ecs:RegisterTaskDefinition",
          "ecs:ListTasks",
          "ecs:DescribeTasks"
        ]
        Resource = "*"
      },
      {
        Effect = "Allow"
        Action = [
          "s3:PutObject",
          "s3:GetObject",
          "s3:ListBucket",
          "s3:DeleteObject"
        ]
        Resource = [
          "arn:aws:s3:::parcel-management-${var.environment}-frontend",
          "arn:aws:s3:::parcel-management-${var.environment}-frontend/*"
        ]
      },
      {
        Effect = "Allow"
        Action = [
          "cloudfront:CreateInvalidation"
        ]
        Resource = "*"
      }
    ]
  })
}

resource "aws_iam_access_key" "cicd" {
  user = aws_iam_user.cicd.name
}
