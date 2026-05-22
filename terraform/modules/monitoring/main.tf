locals {
  dashboard_name = "parcel-management-${var.environment}"
}

resource "aws_cloudwatch_dashboard" "this" {
  count          = var.enable_compute ? 1 : 0
  dashboard_name = local.dashboard_name
  dashboard_body = jsonencode({

    widgets = [
      # ─────────────────────────────────────────────────────────────
      # Row 1 — Health (3 number widgets, 8 columns each)
      # ─────────────────────────────────────────────────────────────
      {
        type   = "metric"
        x      = 0
        y      = 0
        width  = 8
        height = 6
        properties = {
          view    = "singleValue"
          stacked = false
          region  = var.region
          title   = "Healthy Hosts (ALB)"
          metrics = [
            ["AWS/ApplicationELB", "HealthyHostCount",
              "LoadBalancer", var.alb_arn_suffix,
              "TargetGroup", var.target_group_arn_suffix,
              { label = "Healthy", stat = "Average", period = 60 }]
          ]
        }
      },
      {
        type   = "metric"
        x      = 8
        y      = 0
        width  = 8
        height = 6
        properties = {
          view    = "singleValue"
          stacked = false
          region  = var.region
          title   = "Running Tasks (ECS)"
          metrics = [
            ["AWS/ECS", "RunningTaskCount",
              "ClusterName", var.ecs_cluster_name,
              "ServiceName", var.ecs_service_name,
              { label = "Tasks", stat = "Average", period = 60 }]
          ]
        }
      },
      {
        type   = "metric"
        x      = 16
        y      = 0
        width  = 8
        height = 6
        properties = {
          view    = "singleValue"
          stacked = false
          region  = var.region
          title   = "ALB Latency p99"
          metrics = [
            ["AWS/ApplicationELB", "TargetResponseTime",
              "LoadBalancer", var.alb_arn_suffix,
              { label = "p99 (s)", stat = "p99", period = 300 }]
          ]
        }
      },

      # ─────────────────────────────────────────────────────────────
      # Row 2 — ALB Traffic (4 number widgets, 6 columns each)
      # ─────────────────────────────────────────────────────────────
      {
        type   = "metric"
        x      = 0
        y      = 6
        width  = 6
        height = 6
        properties = {
          view    = "singleValue"
          stacked = false
          region  = var.region
          title   = "ALB Requests/min"
          metrics = [
            ["AWS/ApplicationELB", "RequestCount",
              "LoadBalancer", var.alb_arn_suffix,
              { label = "Req/min", stat = "Sum", period = 60 }]
          ]
        }
      },
      {
        type   = "metric"
        x      = 6
        y      = 6
        width  = 6
        height = 6
        properties = {
          view    = "singleValue"
          stacked = false
          region  = var.region
          title   = "ALB Latency avg"
          metrics = [
            ["AWS/ApplicationELB", "TargetResponseTime",
              "LoadBalancer", var.alb_arn_suffix,
              { label = "avg (s)", stat = "Average", period = 300 }]
          ]
        }
      },
      {
        type   = "metric"
        x      = 12
        y      = 6
        width  = 6
        height = 6
        properties = {
          view    = "singleValue"
          stacked = false
          region  = var.region
          title   = "5xx Errors (ALB)"
          metrics = [
            ["AWS/ApplicationELB", "HTTPCode_Target_5XX_Count",
              "LoadBalancer", var.alb_arn_suffix,
              { label = "5xx", stat = "Sum", period = 60 }]
          ]
        }
      },
      {
        type   = "metric"
        x      = 18
        y      = 6
        width  = 6
        height = 6
        properties = {
          view    = "singleValue"
          stacked = false
          region  = var.region
          title   = "4xx Errors (ALB)"
          metrics = [
            ["AWS/ApplicationELB", "HTTPCode_Target_4XX_Count",
              "LoadBalancer", var.alb_arn_suffix,
              { label = "4xx", stat = "Sum", period = 60 }]
          ]
        }
      },

      # ─────────────────────────────────────────────────────────────
      # Row 3 — ECS Container (2 number + 1 line graph)
      # ─────────────────────────────────────────────────────────────
      {
        type   = "metric"
        x      = 0
        y      = 12
        width  = 8
        height = 6
        properties = {
          view    = "singleValue"
          stacked = false
          region  = var.region
          title   = "ECS CPU %"
          metrics = [
            ["AWS/ECS", "CPUUtilization",
              "ClusterName", var.ecs_cluster_name,
              "ServiceName", var.ecs_service_name,
              { label = "CPU", stat = "Average", period = 300 }]
          ]
        }
      },
      {
        type   = "metric"
        x      = 8
        y      = 12
        width  = 8
        height = 6
        properties = {
          view    = "singleValue"
          stacked = false
          region  = var.region
          title   = "ECS Memory %"
          metrics = [
            ["AWS/ECS", "MemoryUtilization",
              "ClusterName", var.ecs_cluster_name,
              "ServiceName", var.ecs_service_name,
              { label = "Memory", stat = "Average", period = 300 }]
          ]
        }
      },
      {
        type   = "metric"
        x      = 16
        y      = 12
        width  = 8
        height = 6
        properties = {
          view    = "timeSeries"
          stacked = false
          region  = var.region
          title   = "Task Count History"
          metrics = [
            ["AWS/ECS", "RunningTaskCount",
              "ClusterName", var.ecs_cluster_name,
              "ServiceName", var.ecs_service_name,
              { label = "Running Tasks", stat = "Average", period = 60 }]
          ]
        }
      },

      # ─────────────────────────────────────────────────────────────
      # Row 4 — CloudFront (3 number widgets)
      # ─────────────────────────────────────────────────────────────
      {
        type   = "metric"
        x      = 0
        y      = 18
        width  = 8
        height = 6
        properties = {
          view    = "singleValue"
          stacked = false
          region  = var.region
          title   = "CDN Requests/min"
          metrics = [
            ["AWS/CloudFront", "Requests",
              "DistributionId", var.cloudfront_distribution_id,
              "Region", "Global",
              { label = "Req/min", stat = "Sum", period = 60 }]
          ]
        }
      },
      {
        type   = "metric"
        x      = 8
        y      = 18
        width  = 8
        height = 6
        properties = {
          view    = "singleValue"
          stacked = false
          region  = var.region
          title   = "CDN 5xx Error Rate %"
          metrics = [
            ["AWS/CloudFront", "5xxErrorRate",
              "DistributionId", var.cloudfront_distribution_id,
              "Region", "Global",
              { label = "5xx %", stat = "Average", period = 300 }]
          ]
        }
      },
      {
        type   = "metric"
        x      = 16
        y      = 18
        width  = 8
        height = 6
        properties = {
          view    = "singleValue"
          stacked = false
          region  = var.region
          title   = "CDN Cache Hit Rate %"
          metrics = [
            ["AWS/CloudFront", "CacheHitRate",
              "DistributionId", var.cloudfront_distribution_id,
              "Region", "Global",
              { label = "Cache %", stat = "Average", period = 300 }]
          ]
        }
      }
    ]

    periodOverride = "auto"
    start          = "-PT3H"
  })
}
