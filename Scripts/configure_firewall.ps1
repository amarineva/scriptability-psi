#Requires -RunAsAdministrator

# Define variables
$port = "18450"
$ruleNamePrefix = "ScriptAbilityPSI_Service"
$inboundRuleName = "${ruleNamePrefix}_Inbound_TCP_${port}"
$inboundDescription = "Allows inbound traffic for ScriptAbility PSI Service on TCP port ${port}."

# Check and create Inbound Rule
Write-Host "Checking for Inbound Firewall Rule: $inboundRuleName..."
$existingInboundRule = Get-NetFirewallRule -DisplayName $inboundRuleName -ErrorAction SilentlyContinue

if ($null -eq $existingInboundRule) {
    Write-Host "Creating Inbound Firewall Rule: $inboundRuleName..."
    New-NetFirewallRule -DisplayName $inboundRuleName `
        -Direction Inbound `
        -Action Allow `
        -Protocol TCP `
        -LocalPort $port `
        -Profile Any `
        -Description $inboundDescription
    Write-Host "Inbound Firewall Rule '$inboundRuleName' created successfully."
} else {
    Write-Host "Inbound Firewall Rule '$inboundRuleName' already exists. No action taken."
}


Write-Host ""
Write-Host "Firewall configuration script finished."