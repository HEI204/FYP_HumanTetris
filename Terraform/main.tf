resource "azurerm_resource_group" "fyp-rg" {
  name     = "fyp-rg-v2"
  location = "East Asia"
}

resource "azurerm_service_plan" "fyp-service-plan" {
  name                = "fyp-service-plan"
  resource_group_name = azurerm_resource_group.fyp-rg.name
  location            = azurerm_resource_group.fyp-rg.location

  os_type  = "Windows"
  sku_name = "Y1"
}

resource "azurerm_windows_function_app" "fyp-function-app" {
  name                = "fyp-function-app"
  resource_group_name = azurerm_resource_group.fyp-rg.name
  location            = azurerm_resource_group.fyp-rg.location

  storage_account_name       = module.azure_storage.storage_account_name
  storage_account_access_key = module.azure_storage.storage_account_access_key

  service_plan_id = azurerm_service_plan.fyp-service-plan.id

  site_config {
    application_stack {
      dotnet_version = "6"
    }

    cors {
      allowed_origins = ["*"]
    }
  }

  lifecycle {
    ignore_changes = [sticky_settings, tags, site_config]
  }
}

resource "azurerm_function_app_function" "fyp-app-function" {
  name            = "UploadPlayerData"
  function_app_id = azurerm_windows_function_app.fyp-function-app.id
  language        = "CSharp"

  file {
    name    = "run.csx"
    content = file("./function_app_code/UploadPlayerData.csx")
  }

  file {
    name    = "function.proj"
    content = file("./function_app_code/function.proj")
  }

  config_json = jsonencode({
    "bindings" = [
      {
        "authLevel" = "function"
        "direction" = "in"
        "methods" = [
          "post",
        ]
        "name" = "req"
        "type" = "httpTrigger"
      },
      {
        "direction" = "out"
        "name"      = "$return"
        "type"      = "http"
      },
    ]
  })

  lifecycle {
    ignore_changes = [test_data, file]
  }
}

module "azure_storage" {
  source = "./module/storage"

  resource_group_name = azurerm_resource_group.fyp-rg.name
  location            = azurerm_resource_group.fyp-rg.location

  stoarge_account_name             = "fypstorage224"
  storage_account_tier             = "Standard"
  storage_account_replication_type = "LRS"

  stoarge_table_name = "fyptablestorage"
}

module "azure_containerized_web_app" {
  source = "./module/web_service"

  resource_group_name = azurerm_resource_group.fyp-rg.name
  location            = azurerm_resource_group.fyp-rg.location

  acr_name          = "fypmodelwebappacr"
  service_plan_name = "fyp-model-webapp-service-plan"
  web_app_name      = "fypmodelwebapp"
  dockerfile_path   = "../ContainerizedModelFlaskAPI"
}