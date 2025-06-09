#Requires -RunAsAdministrator

# Define the display name of the firewall rule to be removed (only inbound rule is created)
$inboundRuleName = "ScriptAbilityPSI_Service_Inbound_TCP_18450"

# Function to remove a firewall rule if it exists
function Remove-FirewallRuleIfExists {
    param (
        [string]$RuleDisplayName
    )

    try {
        $rule = Get-NetFirewallRule -DisplayName $RuleDisplayName -ErrorAction SilentlyContinue
        if ($null -ne $rule) {
            Write-Host "Attempting to remove firewall rule: '$RuleDisplayName'..."
            Remove-NetFirewallRule -DisplayName $RuleDisplayName -ErrorAction Stop
            Write-Host "Firewall rule '$RuleDisplayName' removed successfully."
        } else {
            Write-Host "Firewall rule '$RuleDisplayName' not found. No action taken."
        }
    } catch {
        Write-Error "An error occurred while trying to remove rule '$RuleDisplayName': $($_.Exception.Message)"
    }
}

# Remove the Inbound Rule
Remove-FirewallRuleIfExists -RuleDisplayName $inboundRuleName

Write-Host "Firewall rule removal process completed."