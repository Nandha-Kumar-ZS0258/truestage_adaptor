variable "location" {
  description = "Azure region"
  type        = string
  default     = "East US"
}

variable "resource_group_name" {
  description = "Name of the Azure Resource Group"
  type        = string
  default     = "rg-truestage-poc"
}

variable "environment" {
  description = "Environment name"
  type        = string
  default     = "poc"
}

variable "sql_admin_login" {
  description = "SQL Server admin login"
  type        = string
  sensitive   = true
}

variable "sql_admin_password" {
  description = "SQL Server admin password"
  type        = string
  sensitive   = true
}
