switch ((Test-Path -Path ".\build\")) {
    $true {
        Remove-Item -Path ".\build" -Recurse -Force
        break
    }
}

$null = New-Item -Path ".\build" -ItemType "Directory"

Push-Location -Path ".\src\graph-email\"

dotnet clean
dotnet publish /property:PublishWithAspNetCoreTargetManifest=false

Pop-Location

Push-Location -Path ".\src\password-expiration\"

dotnet clean
dotnet publish /property:PublishWithAspNetCoreTargetManifest=false

Pop-Location

Copy-Item -Path ".\src\PasswordExpirationEmail" -Destination ".\build\" -Recurse -Force

Copy-Item -Path ".\src\graph-email\bin\Debug\netstandard2.0\publish\graph-email.dll" -Destination ".\build\PasswordExpirationEmail\"
Copy-Item -Path ".\src\graph-email\bin\Debug\netstandard2.0\publish\Microsoft.Identity.Client.dll" -Destination ".\build\PasswordExpirationEmail\"
Copy-Item -Path ".\src\graph-email\bin\Debug\netstandard2.0\publish\Newtonsoft.Json.dll" -Destination ".\build\PasswordExpirationEmail\"

Copy-Item -Path ".\src\password-expiration\bin\Debug\netstandard2.0\publish\password-expiration.dll" -Destination ".\build\PasswordExpirationEmail\"