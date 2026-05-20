# ============================================================
# GitHub Actions OIDC — password-less AWS auth
# ============================================================
#
# GitHub generates an OIDC token for each workflow run.
# This provider makes AWS trust those tokens.
# Create once per AWS account (production only, or done manually).
#
resource "aws_iam_openid_connect_provider" "github" {
  count = var.create_oidc_provider ? 1 : 0

  url             = "https://token.actions.githubusercontent.com"
  client_id_list  = ["sts.amazonaws.com"]
  thumbprint_list = ["6938fd4d98bab03faadb97b34396831e3780aea1"]

  tags = var.tags
}

# Build the provider ARN ourselves — avoids cross-environment data source issues.
# Format: arn:aws:iam::<account>:oidc-provider/token.actions.githubusercontent.com
locals {
  oidc_provider_arn = "arn:aws:iam::${data.aws_caller_identity.current.account_id}:oidc-provider/token.actions.githubusercontent.com"
}

data "aws_caller_identity" "current" {}

# ============================================================
# Trust policy — only the designated branch can assume this role
# ============================================================
data "aws_iam_policy_document" "assume_role" {
  statement {
    effect  = "Allow"
    actions = ["sts:AssumeRoleWithWebIdentity"]

    principals {
      type        = "Federated"
      identifiers = [local.oidc_provider_arn]
    }

    condition {
      test     = "StringLike"
      variable = "token.actions.githubusercontent.com:sub"
      values = [
        "repo:${var.github_repo}:ref:refs/heads/${var.allowed_branch}",
        "repo:${var.github_repo}:ref:refs/heads/${var.allowed_branch}:*",
      ]
    }

    condition {
      test     = "StringEquals"
      variable = "token.actions.githubusercontent.com:aud"
      values   = ["sts.amazonaws.com"]
    }
  }
}

# ============================================================
# IAM role — AdministratorAccess, scoped by OIDC trust
# ============================================================
resource "aws_iam_role" "github_actions" {
  name               = "github-actions-${var.environment}"
  assume_role_policy = data.aws_iam_policy_document.assume_role.json

  tags = var.tags
}

resource "aws_iam_role_policy_attachment" "admin" {
  role       = aws_iam_role.github_actions.name
  policy_arn = "arn:aws:iam::aws:policy/AdministratorAccess"
}
