#Requires -Version 7.0
# Settings window for the Nomi Can't Dance BepInEx mod.
# Edits BepInEx\config\com.community.hs.NomiCantDance.cfg. Changes apply on next Hearthstone start.
param([string]$HearthstoneDir = 'C:\Program Files (x86)\Hearthstone')

Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName System.Drawing

$CfgPath = Join-Path $HearthstoneDir 'BepInEx\config\com.community.hs.NomiCantDance.cfg'
$LogPath = Join-Path $HearthstoneDir 'BepInEx\LogOutput.log'

function Get-CfgValue($lines, $section, $key) {
    $inSection = $false
    foreach ($line in $lines) {
        $t = $line.Trim()
        if ($t -match '^\[(.+)\]$') { $inSection = ($Matches[1] -eq $section); continue }
        if ($inSection -and $t -match "^$key\s*=\s*(.*)$") { return $Matches[1].Trim() }
    }
    return $null
}

function Set-CfgValue($lines, $section, $key, $value) {
    $inSection = $false
    for ($i = 0; $i -lt $lines.Count; $i++) {
        $t = $lines[$i].Trim()
        if ($t -match '^\[(.+)\]$') { $inSection = ($Matches[1] -eq $section); continue }
        if ($inSection -and $t -match "^$key\s*=") { $lines[$i] = "$key = $value"; return $true }
    }
    return $false
}

function Read-Cfg {
    if (-not (Test-Path $CfgPath)) { throw "Config not found: $CfgPath. Is the mod installed?" }
    return [System.Collections.Generic.List[string]](Get-Content $CfgPath)
}

function Write-Cfg($lines) {
    $text = ($lines -join "`r`n") + "`r`n"
    [System.IO.File]::WriteAllText($CfgPath, $text, [System.Text.UTF8Encoding]::new($false))
}

$lines = Read-Cfg
$form = New-Object System.Windows.Forms.Form
$form.Text = "Nomi Can't Dance"
$form.ClientSize = New-Object System.Drawing.Size(420, 300)
$form.StartPosition = 'CenterScreen'
$form.FormBorderStyle = 'FixedDialog'
$form.MaximizeBox = $false

$tabs = New-Object System.Windows.Forms.TabControl
$tabs.Dock = 'Fill'
$form.Controls.Add($tabs)

$fixTab = New-Object System.Windows.Forms.TabPage 'Fix'
$aboutTab = New-Object System.Windows.Forms.TabPage 'About'
$tabs.TabPages.AddRange(@($fixTab, $aboutTab))

$enabled = New-Object System.Windows.Forms.CheckBox
$enabled.Text = 'Enable board fix (minion dance)'
$enabled.Location = New-Object System.Drawing.Point(16, 16)
$enabled.Size = New-Object System.Drawing.Size(380, 24)
$enabled.Checked = (Get-CfgValue $lines 'Fix' 'Enabled') -eq 'true'

$logFixes = New-Object System.Windows.Forms.CheckBox
$logFixes.Text = 'Log each corrected slot to LogOutput.log'
$logFixes.Location = New-Object System.Drawing.Point(16, 48)
$logFixes.Size = New-Object System.Drawing.Size(380, 24)
$logFixes.Checked = (Get-CfgValue $lines 'Fix' 'LogFixes') -eq 'true'

$status = New-Object System.Windows.Forms.Label
$status.Location = New-Object System.Drawing.Point(16, 90)
$status.Size = New-Object System.Drawing.Size(380, 60)

$save = New-Object System.Windows.Forms.Button
$save.Text = 'Save'
$save.Location = New-Object System.Drawing.Point(16, 170)
$save.Size = New-Object System.Drawing.Size(100, 32)

$openLog = New-Object System.Windows.Forms.Button
$openLog.Text = 'Open log'
$openLog.Location = New-Object System.Drawing.Point(126, 170)
$openLog.Size = New-Object System.Drawing.Size(100, 32)

$fixTab.Controls.AddRange(@($enabled, $logFixes, $status, $save, $openLog))

$aboutText = New-Object System.Windows.Forms.Label
$aboutText.Dock = 'Fill'
$aboutText.Padding = New-Object System.Windows.Forms.Padding(12)
$aboutText.Text = "Nomi Can't Dance v1.0.0 (BepInEx mod, com.community.hs.NomiCantDance)`r`n`r`nConfig: $CfgPath`r`nSource: https://github.com/EliasDevSE/NomiCantDance`r`n`r`nSettings take effect when Hearthstone starts."
$aboutTab.Controls.Add($aboutText)

function Update-Status {
    $running = [bool](Get-Process -Name Hearthstone -ErrorAction SilentlyContinue)
    $status.Text = if ($running) { 'Hearthstone is running. Restart it to apply saved changes.' } else { 'Hearthstone is not running. Changes apply on next start.' }
}

$save.Add_Click({
    try {
        $fresh = Read-Cfg
        Set-CfgValue $fresh 'Fix' 'Enabled' ($enabled.Checked.ToString().ToLower()) | Out-Null
        Set-CfgValue $fresh 'Fix' 'LogFixes' ($logFixes.Checked.ToString().ToLower()) | Out-Null
        Write-Cfg $fresh
        Update-Status
        [System.Windows.Forms.MessageBox]::Show('Saved.', "Nomi Can't Dance") | Out-Null
    } catch {
        [System.Windows.Forms.MessageBox]::Show($_.Exception.Message, 'Save failed') | Out-Null
    }
})

$openLog.Add_Click({
    if (Test-Path $LogPath) { Start-Process notepad.exe $LogPath } else { $status.Text = "No log at $LogPath" }
})

Update-Status
[System.Windows.Forms.Application]::Run($form)
