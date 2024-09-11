output "storage_account_name" {
  value = azurerm_storage_account.fyp-stoarge-ac.name
}

output "storage_account_access_key" {
  value = azurerm_storage_account.fyp-stoarge-ac.primary_access_key
}

output "storage_account_id" {
  value = azurerm_storage_account.fyp-stoarge-ac.id
}
