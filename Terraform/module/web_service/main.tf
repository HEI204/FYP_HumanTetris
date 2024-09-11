# Set container image name
locals {
  image_name = "fyp-model-webapp-image"
  image_tag  = "latest"
}

resource "azurerm_container_registry" "fypmodelwebapp-container" {
  name                = var.acr_name
  resource_group_name = var.resource_group_name
  location            = var.location
  sku                 = "Basic"
  admin_enabled       = true
}

resource "azurerm_service_plan" "fypmodelwebapp-service-plan" {
  name                = var.service_plan_name
  resource_group_name = var.resource_group_name
  location            = var.location
  os_type             = "Linux"
  sku_name            = "B1"
}

# Create docker image
resource "null_resource" "docker_image" {
  triggers = {
    image_name         = local.image_name
    image_tag          = local.image_tag
    registry_name      = azurerm_container_registry.fypmodelwebapp-container.name
    dockerfile_context = var.dockerfile_path
    dir_sha1           = sha1(join("", [for f in fileset(path.cwd, "docker/*") : filesha1(f)]))
  }
  provisioner "local-exec" {
    command     = "./scripts/docker_build_and_push_to_acr.sh ${var.resource_group_name} ${self.triggers.image_name} ${self.triggers.image_tag} ${self.triggers.registry_name} ${self.triggers.dockerfile_context}"
    interpreter = ["bash", "-c"]
  }
}

resource "azurerm_linux_web_app" "fypmodelwebapp" {
  name                = var.web_app_name
  resource_group_name = var.resource_group_name
  location            = var.location
  service_plan_id     = azurerm_service_plan.fypmodelwebapp-service-plan.id
  https_only          = true

  site_config {
    application_stack {
      docker_image     = "${azurerm_container_registry.fypmodelwebapp-container.login_server}/${local.image_name}"
      docker_image_tag = local.image_tag
    }

    always_on         = true
    ftps_state        = "FtpsOnly"
    health_check_path = "/health"
  }

  app_settings = {
    "WEBSITES_PORT"                   = "50505"
    "DOCKER_REGISTRY_SERVER_URL"      = azurerm_container_registry.fypmodelwebapp-container.login_server
    "DOCKER_REGISTRY_SERVER_USERNAME" = azurerm_container_registry.fypmodelwebapp-container.admin_username
    "DOCKER_REGISTRY_SERVER_PASSWORD" = azurerm_container_registry.fypmodelwebapp-container.admin_password
  }

  identity {
    type = "SystemAssigned"
  }

  lifecycle {
    ignore_changes = [
      site_config.0.application_stack,
      app_settings.DOCKER_REGISTRY_SERVER_USERNAME,
      app_settings.DOCKER_REGISTRY_SERVER_PASSWORD
    ]
  }

  depends_on = [
    null_resource.docker_image
  ]

}

resource "azurerm_role_assignment" "acr" {
  scope                = azurerm_container_registry.fypmodelwebapp-container.id
  role_definition_name = "AcrPull"
  principal_id         = azurerm_linux_web_app.fypmodelwebapp.identity[0].principal_id
}
