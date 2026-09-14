variable "aws_region" {
  description = "Only us-east-1 is authorized for this restricted Free-plan account."
  type        = string
  default     = "us-east-1"

  validation {
    condition     = var.aws_region == "us-east-1"
    error_message = "This project currently permits only us-east-1."
  }
}

variable "aws_profile" {
  description = "Local AWS CLI profile holding temporary aws login credentials; never a secret value."
  type        = string
  default     = "new-profile-name"

  validation {
    condition     = length(trimspace(var.aws_profile)) > 0
    error_message = "Select a local AWS CLI profile."
  }
}
