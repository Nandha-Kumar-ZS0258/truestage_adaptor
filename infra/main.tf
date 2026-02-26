# ============================================================
# TrueStage POC — Azure Infrastructure (Terraform)
# Provision once. All CUs share this infrastructure.
# ============================================================

terraform {
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 3.0"
    }
  }
}

provider "azurerm" {
  features {}
}

# ── Resource Group ─────────────────────────────────────────

resource "azurerm_resource_group" "main" {
  name     = var.resource_group_name
  location = var.location
}

# ── Storage Account (Blob) ─────────────────────────────────

resource "azurerm_storage_account" "main" {
  name                     = "satruestage${var.environment}"
  resource_group_name      = azurerm_resource_group.main.name
  location                 = azurerm_resource_group.main.location
  account_tier             = "Standard"
  account_replication_type = "LRS"
}

# Blob containers
resource "azurerm_storage_container" "incoming" {
  name                  = "incoming"
  storage_account_name  = azurerm_storage_account.main.name
  container_access_type = "private"
}

resource "azurerm_storage_container" "raw" {
  name                  = "raw"
  storage_account_name  = azurerm_storage_account.main.name
  container_access_type = "private"
}

resource "azurerm_storage_container" "configs" {
  name                  = "configs"
  storage_account_name  = azurerm_storage_account.main.name
  container_access_type = "private"
}

resource "azurerm_storage_container" "processed" {
  name                  = "processed"
  storage_account_name  = azurerm_storage_account.main.name
  container_access_type = "private"
}

resource "azurerm_storage_container" "failed" {
  name                  = "failed"
  storage_account_name  = azurerm_storage_account.main.name
  container_access_type = "private"
}

# ── Service Bus ────────────────────────────────────────────

resource "azurerm_servicebus_namespace" "main" {
  name                = "sb-truestage-${var.environment}"
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name
  sku                 = "Standard"
}

resource "azurerm_servicebus_topic" "file_arrived" {
  name         = "file-arrived"
  namespace_id = azurerm_servicebus_namespace.main.id
}

resource "azurerm_servicebus_topic" "ingestion_ready" {
  name         = "ingestion-ready"
  namespace_id = azurerm_servicebus_namespace.main.id
}

resource "azurerm_servicebus_topic" "ingestion_started" {
  name         = "ingestion-started"
  namespace_id = azurerm_servicebus_namespace.main.id
}

resource "azurerm_servicebus_topic" "ingestion_completed" {
  name         = "ingestion-completed"
  namespace_id = azurerm_servicebus_namespace.main.id
}

resource "azurerm_servicebus_topic" "ingestion_failed" {
  name         = "ingestion-failed"
  namespace_id = azurerm_servicebus_namespace.main.id
}

# ── Azure SQL ──────────────────────────────────────────────

resource "azurerm_mssql_server" "main" {
  name                         = "sql-truestage-${var.environment}"
  resource_group_name          = azurerm_resource_group.main.name
  location                     = azurerm_resource_group.main.location
  version                      = "12.0"
  administrator_login          = var.sql_admin_login
  administrator_login_password = var.sql_admin_password
}

resource "azurerm_mssql_database" "main" {
  name      = "db-truestage"
  server_id = azurerm_mssql_server.main.id
  sku_name  = "S1"
}

# ── Key Vault ──────────────────────────────────────────────

resource "azurerm_key_vault" "main" {
  name                = "kv-truestage-${var.environment}"
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name
  tenant_id           = data.azurerm_client_config.current.tenant_id
  sku_name            = "standard"
}

data "azurerm_client_config" "current" {}

# ── App Service Plan (for Azure Functions) ─────────────────

resource "azurerm_service_plan" "main" {
  name                = "asp-truestage-${var.environment}"
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name
  os_type             = "Linux"
  sku_name            = "Y1"  # Consumption plan
}

# ── Application Insights ───────────────────────────────────

resource "azurerm_application_insights" "main" {
  name                = "ai-truestage-${var.environment}"
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name
  application_type    = "web"
}
