resource "azurerm_storage_account" "fyp-stoarge-ac" {
  name                = var.stoarge_account_name
  resource_group_name = var.resource_group_name
  location            = var.location

  account_tier             = var.storage_account_tier
  account_replication_type = var.storage_account_replication_type
}

resource "azurerm_storage_table" "fyp-table-storage" {
  name                 = var.stoarge_table_name
  storage_account_name = azurerm_storage_account.fyp-stoarge-ac.name
}