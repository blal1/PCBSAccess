param (
    [Parameter(Mandatory=$false)]
    [string]$ModName,

    [Parameter(Mandatory=$false)]
    [string]$Namespace,

    [Parameter(Mandatory=$false)]
    [string]$GameName,

    [Parameter(Mandatory=$false)]
    [ValidateSet("BepInEx", "MelonLoader")]
    [string]$Loader
)

# Function to ask for input if not provided
function Get-Input {
    param($Prompt, $DefaultValue)
    $val = Read-Host "$Prompt (Default: $DefaultValue)"
    if ([string]::IsNullOrWhiteSpace($val)) {
        return $DefaultValue
    }
    return $val
}

# Ensure output directory exists for results
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$ProjectRoot = Split-Path -Parent $ScriptDir
$OutputBase = Join-Path $ProjectRoot "output"

# Interactive input if parameters are missing
if ([string]::IsNullOrWhiteSpace($ModName)) {
    $ModName = Read-Host "Enter Mod Name (e.g., MyAccessibilityMod)"
}
if ([string]::IsNullOrWhiteSpace($Namespace)) {
    $Namespace = Read-Host "Enter Namespace (e.g., MyMod)"
}
if ([string]::IsNullOrWhiteSpace($GameName)) {
    $GameName = Read-Host "Enter Game Name (e.g., Pet Idle)"
}
if ([string]::IsNullOrWhiteSpace($Loader)) {
    do {
        $LoaderInput = Read-Host "Enter Loader (BepInEx/MelonLoader)"
        if ($LoaderInput -eq "BepInEx") { $Loader = "BepInEx" }
        elseif ($LoaderInput -eq "MelonLoader") { $Loader = "MelonLoader" }
    } while ([string]::IsNullOrWhiteSpace($Loader))
}

$PluginGuid = "$Namespace.$ModName"
$PluginVersion = "1.0.0"
$OutputDir = Join-Path $OutputBase $ModName
$TemplatesDir = Join-Path $ProjectRoot "templates"
$LoaderTemplatesDir = Join-Path $TemplatesDir $Loader.ToLower()
$SharedTemplatesDir = Join-Path $TemplatesDir "shared"

Write-Host "`nScaffolding $ModName for $GameName ($Loader)..." -ForegroundColor Cyan

# Check if templates exist
if (-not (Test-Path $LoaderTemplatesDir)) {
    Write-Error "Templates for $Loader not found at $LoaderTemplatesDir"
    exit 1
}

# Create output directory
if (-not (Test-Path $OutputBase)) {
    New-Item -ItemType Directory -Path $OutputBase -Force | Out-Null
}

if (Test-Path $OutputDir) {
    Write-Host "Output directory $OutputDir already exists. Overwriting files..." -ForegroundColor Yellow
} else {
    New-Item -ItemType Directory -Path $OutputDir -Force | Out-Null
}

# Define placeholders
$Placeholders = @{
    "NAMESPACE" = $Namespace
    "PLUGIN_NAME" = $ModName
    "MODNAME" = $ModName
    "PLUGIN_GUID" = $PluginGuid
    "PLUGIN_VERSION" = $PluginVersion
    "GAME_NAME" = $GameName
    "SPIELNAME" = $GameName
    "SPIELORDNER" = "C:\Path\To\Your\Game"
    "TARGET_FRAMEWORK" = "net472"
    "RUNTIME_ORDNER" = "net35"
    "AUTHOR" = "YourName"
    "DEVELOPER" = "GameDeveloper"
}

# Copy files and replace placeholders
$filesToCopy = Get-ChildItem -Path $LoaderTemplatesDir -Filter "*.template"
$filesToCopy += Get-ChildItem -Path $SharedTemplatesDir -Filter "*.template"

foreach ($file in $filesToCopy) {
    try {
        $content = Get-Content -Path $file.FullName -Raw -ErrorAction Stop

        # Replace all placeholders
        foreach ($key in $Placeholders.Keys) {
            $content = $content.Replace($key, $Placeholders[$key])
        }

        # New filename
        $newName = $file.Name -replace "\.template$", ""
        if ($newName -eq "csproj") {
            $newName = "$ModName.csproj"
        }

        $outputPath = Join-Path $OutputDir $newName
        $content | Set-Content -Path $outputPath -ErrorAction Stop
        Write-Host "  Created $newName"
    }
    catch {
        Write-Error "Failed to process $($file.Name): $($_.Exception.Message)"
    }
}

Write-Host "`nSuccessfully scaffolded $ModName!" -ForegroundColor Green
Write-Host "Location: $OutputDir"
Write-Host "Next steps:"
Write-Host "1. Open $OutputDir\$ModName.csproj in Visual Studio or VS Code"
Write-Host "2. Update SPIELORDNER in the .csproj file to point to your game folder"
Write-Host "3. Build the project"
