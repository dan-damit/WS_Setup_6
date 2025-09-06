Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName System.Drawing

# Create form
$form = New-Object System.Windows.Forms.Form
$form.Text = "WS Setup Builder"
$form.Size = New-Object System.Drawing.Size(600, 400)
$form.StartPosition = "CenterScreen"

# Build Button
$buildButton = New-Object System.Windows.Forms.Button
$buildButton.Text = "Build Setup"
$buildButton.Size = New-Object System.Drawing.Size(100, 30)
$buildButton.Location = New-Object System.Drawing.Point(20, 20)
$form.Controls.Add($buildButton)

# Close Button
$closeButton = New-Object System.Windows.Forms.Button
$closeButton.Text = "Close"
$closeButton.Size = New-Object System.Drawing.Size(100, 30)
$closeButton.Location = New-Object System.Drawing.Point(140, 20)
$closeButton.Add_Click({ $form.Close() })
$form.Controls.Add($closeButton)

# Status Label
$statusLabel = New-Object System.Windows.Forms.Label
$statusLabel.Text = "Ready"
$statusLabel.AutoSize = $true
$statusLabel.Location = New-Object System.Drawing.Point(260, 27)
$form.Controls.Add($statusLabel)

# Log TextBox
$UpdateStatusBox = New-Object System.Windows.Forms.TextBox
$UpdateStatusBox.Multiline = $true
$UpdateStatusBox.ScrollBars = "Vertical"
$UpdateStatusBox.ReadOnly = $true
$UpdateStatusBox.Size = New-Object System.Drawing.Size(540, 280)
$UpdateStatusBox.Location = New-Object System.Drawing.Point(20, 60)
$UpdateStatusBox.Anchor = "Top, Bottom, Left, Right"
$form.Controls.Add($UpdateStatusBox)

# Update Status func
function UpdateStatus($msg) {
    $statusLabel.Text = $msg
    $UpdateStatusBox.AppendText("[$(Get-Date -Format 'HH:mm:ss')] $msg`r`n")
}

# Build UI
$buildButton.Add_Click({
    $buildButton.Enabled = $false
    UpdateStatus "Starting build..."

    try {
        $projectRoot = "C:\Users\dan\WS_Setup_6"
        $foldersToClean = Get-ChildItem -Path $projectRoot -Recurse -Directory |
            Where-Object { $_.Name -in @("bin", "obj") }

        foreach ($folder in $foldersToClean) {
            try {
                Remove-Item -Path $folder.FullName -Recurse -Force -ErrorAction Stop
                UpdateStatus "Deleted: $($folder.FullName)"
            } catch {
                UpdateStatus "Failed to delete: $($folder.FullName) — $_"
            }
        }

        $paths = @(
            "WS_Setup_6.UI",
            "WS_Setup_6.MSI",
            "WS_Setup_6.Bundle"
        )

		# Building UI
		UpdateStatus "Building .UI.exe"
        Push-Location "$projectRoot\WS_Setup_6.UI"
		dotnet clean
		dotnet restore
		dotnet build -c Release
		dotnet publish -c Release
		Pop-Location
		
		# Sign .UI.exe
		UpdateStatus "Signing .UI.exe"
		signtool sign /fd SHA256 /f "C:\Users\dan\OneDrive\ADV_TECH\Scripts\Certs\SignCode_Expires_20260709.pfx" /p St@ff1234! /tr http://timestamp.digicert.com /td SHA256 "C:\Users\dan\WS_Setup_6\WS_Setup_6.UI\bin\Release\net8.0-windows\win-x64\Publish\WS_Setup_6.UI.exe"
		
		# Build MSI
		UpdateStatus "Building MSI"
		Push-Location "$projectRoot\WS_Setup_6.MSI"
		dotnet clean
		dotnet restore
		dotnet build -c Release
		dotnet publish -c Release
		Pop-Location
		
		# Sign MSI
		UpdateStatus "Signing MSI"
		signtool sign /fd SHA256 /f "C:\Users\dan\OneDrive\ADV_TECH\Scripts\Certs\SignCode_Expires_20260709.pfx" /p St@ff1234! /tr http://timestamp.digicert.com /td SHA256 "C:\Users\dan\WS_Setup_6\WS_Setup_6.MSI\Deploy\Release\en-us\WS_Setup_6.MSI.msi"
		
		# Build Bootstrapper
		UpdateStatus "Building Final Bundle"
		Push-Location "$projectRoot\WS_Setup_6.Bundle"
		dotnet clean
		dotnet restore
		dotnet build -c Release
		dotnet publish -c Release
		Pop-Location

        UpdateStatus "Build complete."
    } catch {
        $_ | Out-File "$env:USERPROFILE\WS_Setup_Build_Error.Log" -Append
        UpdateStatus "Build failed. See UpdateStatus file for details."
    }

    $buildButton.Enabled = $true
})

# Run form
[void]$form.ShowDialog()