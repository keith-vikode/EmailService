Properties {
    $dotnetPath = "$pwd\.dotnetcli"
    $dotnet = "$dotnetPath\dotnet.exe"
}

$projectFileName = "project.json"
$solutionRoot = (get-item $PSScriptRoot).parent.fullname
$artifactsRoot = "$solutionRoot\artifacts"
$srcRoot = "$solutionRoot\src"
$testsRoot = "$solutionRoot\test"
$globalFilePath = "$solutionRoot\global.json"
$appProjects = Get-ChildItem "$srcRoot\**\$projectFileName" | foreach { $_.FullName }
$testProjects = Get-ChildItem "$testsRoot\**\$projectFileName" | foreach { $_.FullName }

Task default -Depends install, Compile
Task install -Depends Install-DotNet, Install-GulpBower

function Say {
    param(
        [Parameter(Mandatory = $true)]
        [string]$message
    )
    process {
        Write-Host "BUILD: $message" -ForegroundColor Cyan
    }
}

Task Install-DotNet {
    # prefix the file with a . so that it'll be ignored by Git, but don't remove
    # it so that next time we can save the download time
    $installScript = ".\.dotnet-install.ps1"

    # Download the "dotnet-install.ps1" script.
    Invoke-WebRequest "https://raw.githubusercontent.com/dotnet/cli/rel/1.0.0-preview2/scripts/obtain/dotnet-install.ps1" -OutFile $installScript

    # Decide to install into a ".dotnetcli" local directory.
    $env:DOTNET_INSTALL_DIR = $dotnetPath

    # Do the actual install.
    & $installScript -Channel "preview" -Version "1.0.0-preview2-003121" -InstallDir "$env:DOTNET_INSTALL_DIR"

    #Remove-Item $installScript
}

Task Install-GulpBower {
    if (-Not [bool](Get-Command node -errorAction SilentlyContinue)) {
        throw "Node is not installed."
    } else {
        $env:path += ";$env:APPDATA\npm" # configure local path
    }

    if(-Not [bool](Get-Command npm -errorAction SilentlyContinue)) {
        throw "NPM is not installed."
    }

    if(-Not [bool](Get-Command bower -errorAction SilentlyContinue)) {
        Say "Installing Bower..."
        npm install -global bower
    }

    if(-Not [bool](Get-Command gulp -errorAction SilentlyContinue)) {
        Say "Installing Gulp..."
        npm install -global gulp
    }
}

Task Compile -Depends Clean, Restore {
	$appProjects | foreach {
		& dotnet build "$_" --configuration $configuration
    }
    
    $testProjects | foreach {
		& dotnet build "$_" --configuration $configuration
    }

    if ($LASTEXITCODE -ne 0) {
        throw "dotnet build failed with exit code $LASTEXITCODE"
    }
}

Task Test -depends Restore, Clean {
	$testProjects | foreach {
		Write-Output "Running tests for '$_'"
		dotnet test  "$_"  
	}
}

Task Restore {
	@($srcRoot, $testsRoot) | foreach {
        Write-Output "Restoring for '$_'"
        & dotnet restore "$_"
    }
}

Task Clean {
   	(Get-ChildItem -Path $solutionRoot -Include bin,obj -Recurse) |
		Foreach-Object {
            Write-Host "Cleaning $_" -ForegroundColor Gray
            Remove-Item $_.FullName -Recurse -Force
		}
 }