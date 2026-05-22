environment            = "staging"

cluster_name           = "parcel-management-staging"
task_definition_family = "parcel-management-staging"
ecs_service_name       = "parcel-management-staging"

task_cpu    = "256"
task_memory = "512"
ecs_service_desired_count = 1

enable_compute = true

vpc_cidr             = "10.1.0.0/16"
public_subnet_cidrs  = ["10.1.1.0/24", "10.1.2.0/24"]
private_subnet_cidrs = ["10.1.101.0/24", "10.1.102.0/24"]
availability_zones   = ["ap-southeast-1a", "ap-southeast-1b"]

tags = {
  Environment = "staging"
  Project     = "parcel-management"
  ManagedBy   = "terraform"
}
