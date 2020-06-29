switch ((Test-Path -Path ".\build\")) {
    $true {
        Remove-Item -Path ".\build" -Recurse -Force
        break
    }
}

$null = New-Item -Path ".\build" -ItemType "Directory"

Push-Location -Path ".\src\graph-email\"

Start-Process -FilePath "dotnet" -ArgumentList @("clean") -Wait
Start-Process -FilePath "dotnet" -ArgumentList @("publish") -Wait

Pop-Location

Push-Location -Path ".\src\password-expiration-classes\"

Start-Process -FilePath "dotnet" -ArgumentList @("clean") -Wait
Start-Process -FilePath "dotnet" -ArgumentList @("publish") -Wait

Pop-Location

Copy-Item -Path ".\src\PasswordExpirationEmail" -Destination ".\build\" -Recurse -Force

Copy-Item -Path ".\src\graph-email\bin\Debug\netstandard2.0\publish\graph-email.dll" -Destination ".\build\PasswordExpirationEmail\"
Copy-Item -Path ".\src\graph-email\bin\Debug\netstandard2.0\publish\Microsoft.Identity.Client.dll" -Destination ".\build\PasswordExpirationEmail\"
Copy-Item -Path ".\src\graph-email\bin\Debug\netstandard2.0\publish\Newtonsoft.Json.dll" -Destination ".\build\PasswordExpirationEmail\"

Copy-Item -Path ".\src\password-expiration-classes\bin\Debug\netstandard2.0\publish\password-expiration-classes.dll" -Destination ".\build\PasswordExpirationEmail\"