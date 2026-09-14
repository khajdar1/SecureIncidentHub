# Resource-free foundation. init/validate do not authenticate to AWS.
# No backend, data source or resource is configured in Phase 0.
provider "aws" {
  region  = var.aws_region
  profile = var.aws_profile

  default_tags {
    tags = {
      Project     = "SecureIncidentHub"
      Environment = "lab"
      ManagedBy   = "Terraform"
    }
  }
}
