#Requires -Version 5.1

# Get the directory of the current script
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path

# Path to the HTML template
$templatePath = Join-Path -Path $ScriptDir -ChildPath "post_install_info_template.html"

# Path for the temporary HTML file
$tempHtmlPath = Join-Path -Path "C:\ScriptAbilityPSI" -ChildPath "ScriptAbilityPSI_info.html"

# Attempt to find a suitable IPv4 address
$ipAddress = "localhost" # Default value
try {
    $ipInterfaces = Get-NetIPConfiguration -Detailed | Where-Object {
        $_.IPv4DefaultGateway -ne $null -and $_.NetAdapter.Status -eq 'Up' -and $_.NetAdapter.Virtual -eq $false
    }

    if ($ipInterfaces) {
        # Prioritize non-virtual adapters with a gateway
        $primaryInterface = $ipInterfaces | Sort-Object -Property {$_.NetAdapter.ifIndex} | Select-Object -First 1
        if ($primaryInterface) {
            $ipv4Addresses = Get-NetIPAddress -InterfaceIndex $primaryInterface.InterfaceIndex -AddressFamily IPv4 -ErrorAction SilentlyContinue
            if ($ipv4Addresses) {
                $ipAddress = ($ipv4Addresses | Select-Object -First 1).IPAddress
            }
        }
    } else {
        # Fallback if no ideal interface found, try any non-loopback IPv4
        $allIPv4Addresses = Get-NetIPAddress -AddressFamily IPv4 -ErrorAction SilentlyContinue | Where-Object { $_.IPAddress -ne "127.0.0.1" -and $_.InterfaceAlias -notlike "Loopback*" }
        if ($allIPv4Addresses) {
            # Try to pick one that is likely a LAN IP
            $lanIP = $allIPv4Addresses | Where-Object { $_.IPAddress -match "^(192\.168\.|10\.|172\.(1[6-9]|2[0-9]|3[0-1])\.)" } | Select-Object -First 1
            if ($lanIP) {
                $ipAddress = $lanIP.IPAddress
            } else {
                # Pick the first non-loopback if no common LAN pattern matches
                $ipAddress = ($allIPv4Addresses | Select-Object -First 1).IPAddress
            }
        }
    }
}
catch {
    Write-Warning "Could not automatically determine a suitable IP address. Using '$ipAddress'."
}

# Read the HTML template content
if (Test-Path $templatePath) {
    try {
        $htmlContent = Get-Content -Path $templatePath -Raw -ErrorAction Stop
        
        # Replace the placeholder
        $htmlContent = $htmlContent -replace "{{LOCAL_IP_ADDRESS}}", $ipAddress
        
        # Save the modified content to the temporary file
        Set-Content -Path $tempHtmlPath -Value $htmlContent -Encoding UTF8 -ErrorAction Stop
        Write-Host "Installation information page generated at $tempHtmlPath"
    }
    catch {
        Write-Error "An error occurred while processing or opening the HTML file: $($_.Exception.Message)"
        Write-Error "Template path: $templatePath"
        Write-Error "Temporary HTML path: $tempHtmlPath"
        # Fallback: Display info in console if browser fails
        Write-Host "-----------------------------------------------------"
        Write-Host "ScriptAbility PSI Service Successfully Installed!"
        Write-Host "The ScriptAbility PSI service is now running and accessible."
        Write-Host "To configure the integration, please use the following URL in your application:"
        Write-Host "http://$ipAddress`:18450/receive.php"
        Write-Host "Commonly, localhost can also be used: http://localhost:18450/receive.php"
        Write-Host "-----------------------------------------------------"
    }
}
else {
    Write-Error "HTML template file not found at $templatePath"
    Write-Host "-----------------------------------------------------"
    Write-Host "ScriptAbility PSI Service Successfully Installed!"
    Write-Host "The ScriptAbility PSI service is now running and accessible."
    Write-Host "To configure the integration, please use the following URL in your application:"
    Write-Host "http://$ipAddress`:18450/receive.php"
    Write-Host "Commonly, localhost can also be used: http://localhost:18450/receive.php"
    Write-Host "-----------------------------------------------------"
}