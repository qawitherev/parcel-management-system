environment            = "production"

cluster_name           = "parcel-management-production"
task_definition_family = "parcel-management-production"
ecs_service_name       = "parcel-management-production"

task_cpu    = "512"
task_memory = "1024"
ecs_service_desired_count = 1

enable_compute = false

vpc_cidr             = "10.0.0.0/16"
public_subnet_cidrs  = ["10.0.1.0/24", "10.0.2.0/24"]
private_subnet_cidrs = ["10.0.101.0/24", "10.0.102.0/24"]
availability_zones   = ["ap-southeast-1a", "ap-southeast-1b"]

tags = {
  Environment = "production"
  Project     = "parcel-management"
  ManagedBy   = "terraform"
}
