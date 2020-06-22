filter LastLogonandNotNullEmail {
    if (($PSItem.LastLogonDate -gt (Get-Date).AddYears(-1)) -and ($PSItem.EmailAddress)) {
        $PSItem
    }
}

function Get-ExpiringPasswords {
    [CmdletBinding()]
    param(
        [Parameter(Position = 0)]
        [int]$DaysRemaining = 10,
        [Parameter(Position = 1)]
        [int]$MaxAlive = 120,
        [Parameter(Position = 2, Mandatory)]
        [string]$OUPath,
        [Parameter(Position = 3)]
        [string]$Server
    )

    process {

        $AdSplat = @{
            "Filter"     = { (Enabled -eq $true) };
            "SearchBase" = $OUPath;
            "Properties" = @("PasswordLastSet", "EmailAddress", "LastLogonDate")
        }

        if ($Server) {
            $AdSplat.Add("Server", $Server)
        }

        Write-Progress -Id 1 -Activity "Grabbing Data" -Status "Progress" -CurrentOperation "Grabbing accounts from AD..."
        $Users = Get-ADUser @AdSplat -ErrorAction "Stop" | LastLogonandNotNullEmail

        Write-Progress -Id 1 -Activity "Grabbing Data" -Completed

        $i = 1
        $totalUsers = $Users | Measure-Object | Select-Object -ExpandProperty "Count"

        $currentDateTime = [datetime]::Now

        $parsedData = [System.Collections.Generic.List[PasswordExpiration.Classes.ParsedUserData]]::new()

        foreach ($User in $Users) {
            $foundAccounts = ($parsedData | Where-Object { $PSItem.ExpiringSoon -eq $true } | Measure-Object ).Count
            Write-Progress -Id 2 -Activity "Parsing Data" -Status "Progress: [$($i)/$($totalUsers)] - Found: [$($foundAccounts)]" -CurrentOperation "Parsing data for $($User.Name)..." -PercentComplete ($i / $totalUsers * 100)

            $PasswordExpirationDate = $User.PasswordLastSet.AddDays($MaxAlive)
            $PasswordLife = (New-TimeSpan -Start $User.PasswordLastSet -End $currentDateTime)
            $PasswordExpiresIn = (New-TimeSpan -Start $currentDateTime -End $PasswordExpirationDate)

            $usrObj = [PasswordExpiration.Classes.ParsedUserData]@{
                "SamAccountName"     = $User.SamAccountName;
                "Email"              = $User.EmailAddress;
                "Name"               = "$($User.GivenName) $($User.SurName)";
                "PasswordLastSet"    = $User.PasswordLastSet;
                "PasswordExpiration" = $PasswordExpirationDate;
                "PasswordLife"       = $PasswordLife;
                "PasswordExpiresIn"  = $PasswordExpiresIn;
                "Expired"            = $false;
                "ExpiringSoon"       = $false;
            }

            switch ($PasswordLife.Days -ge $MaxAlive) {
                $true {
                    $usrObj.Expired = $true
                    break
                }
            }

            switch ($PasswordExpiresIn.Days -le $DaysRemaining) {
                $true {
                    $usrObj.ExpiringSoon = $true
                    break
                }
            }

            $parsedData.Add($usrObj)
                    
            $i++
        }
    }
    
    end {
        Write-Progress -Id 2 -Activity "Parsing Data" -Completed
        return $parsedData
    }
}

function New-ExpirationEmail {
    [CmdletBinding(SupportsShouldProcess = $true)]
    param(
        [Parameter(Position = 0, Mandatory)]
        [PasswordExpiration.Classes.ParsedUserData]$Users,
        [Parameter(Position = 1, Mandatory)]
        [string]$EmailAddress,
        [Parameter(Position = 2, Mandatory)]
        [string]$BodyHTML,
        [Parameter(Position = 3)]
        [switch]$DayIntervals,
        [Parameter(Position = 4)]
        [array]$Days = @(10, 5, 2, 1)
    )

    $EmailedUsers = @()

    foreach ($User in $Users) {

        if (!($User.Expired)) {
            if (($DayIntervals -and ($Days -contains $User.PasswordExpiresIn)) -or !($DayIntervals)) {
                Write-Verbose "Emailing $($User.Email)..."
                $Subject = "Alert: Password Expiration Notice ($($User.PasswordExpiresIn.Days) days)"
                

                try {
                    $BodyHTMLSend = $BodyHTML.Replace("`$USERNAME", $User.Name).Replace("`$EXPIREINDAYS", $User.PasswordExpiresIn.Days).Replace("`$EXPIREDATE", (Get-Date $User.PasswordExpiration -Format g))

                    if ($PSCmdlet.ShouldProcess($User.Email, "Send Email")) {

                        Send-GraphMailClientMessage -FromAddress $EmailAddress -ToAddress $User.Email -Subject $Subject -Body $BodyHTMLSend -BodyType "HTML"
                    }

                    Write-Verbose $BodyHTMLSend
                    $EmailedUsers += $User
                }
                catch {
                    Write-Error "There was an error emailing $($User.Email)."
                }
            }
        }
    }
}

function Connect-GraphMailClient {
    [CmdletBinding()]
    param(
        [Parameter(Position = 0, Mandatory)]
        [ValidateNotNullOrEmpty()]
        [string]$ClientId,
        [Parameter(Position = 1, Mandatory)]
        [ValidateNotNullOrEmpty()]
        [string]$TenantId,
        [Parameter(Position = 2, Mandatory)]
        [X509Certificate]$ClientCert
    )

    process {
        $clientConfigSettings = [graph_email.ClientAppConfig]@{
            "ClientId"          = $ClientId;
            "TenantId"          = $TenantId;
            "ClientCertificate" = $ClientCert;
        }

        $clientBuilder = [graph_email.ClientCreator]::new()

        $authToken = $clientBuilder.buildApp($clientConfigSettings)

        $PSCmdlet.SessionState.PSVariable.Set("GraphEmailClientToken", $authToken)

        return $authToken
    }
}

function Send-GraphMailClientMessage {
    [CmdletBinding()]
    param(
        [Parameter(Position = 0, Mandatory)]
        [ValidateNotNullOrEmpty()]
        [string]$FromAddress,
        [Parameter(Position = 1, Mandatory)]
        [ValidateNotNullOrEmpty()]
        [string[]]$ToAddress,
        [Parameter(Position = 2)]
        [string[]]$CcAddress,
        [Parameter(Position = 3, Mandatory)]
        [ValidateNotNullOrEmpty()]
        [string]$Subject,
        [Parameter(Position = 4, Mandatory)]
        [ValidateNotNullOrEmpty()]
        [string]$Body,
        [Parameter(Position = 5, Mandatory)]
        [ValidateNotNullOrEmpty()]
        [string]$BodyType
    )

    process {
        $toAddressList = [System.Collections.Generic.List[graph_email.Mail.Classes.EmailAddress]]::new()

        foreach ($address in $ToAddress) {
            $toAddressList.Add(
                [graph_email.Mail.Classes.EmailAddress]@{
                    "emailAddress" = [graph_email.Mail.Classes.AddressOptions]@{
                        "address" = $address;
                    }
                }
            )
        }

        $ccAddressList = [System.Collections.Generic.List[graph_email.Mail.Classes.EmailAddress]]::new()

        switch (($null = $CcAddress)) {
            $false {
                foreach ($address in $CcAddress) {
                    $ccAddressList.Add(
                        [graph_email.Mail.Classes.EmailAddress]@{
                            "emailAddress" = [graph_email.Mail.Classes.AddressOptions]@{
                                "address" = $address;
                            }
                        }
                    )
                }
                break
            }

            $true {
                Write-Verbose "No addresses were provided for the C.C. parameter."
                break
            }
        }

        $mailMessage = [graph_email.Mail.MailMessage]@{
            "message"         = (
                [graph_email.Mail.Classes.MessageOptions]@{
                    "subject"      = $Subject;
                    "body"         = (
                        [graph_email.Mail.Classes.MessageBody]@{
                            "contentType" = $BodyType;
                            "content"     = $Body;
                        }
                    );
                    "toRecipients" = $toAddressList;
                    "ccRecipients" = $ccAddressList;
                }
            );
            "saveToSentItems" = $false;
        }

        $authToken = $PSCmdlet.SessionState.PSVariable.GetValue("GraphEmailClientToken")

        $restApiHeaders = @{
            "Authorization" = $authToken.CreateAuthorizationHeader();
            "Content-Type"  = "application/json"
        }

        Invoke-RestMethod -Method "Post" -Headers $restApiHeaders -Uri "https://graph.microsoft.com/v1.0/users/$($FromAddress)/sendMail" -Body ($mailMessage | ConvertTo-Json -Depth 4) -ErrorAction Stop
    }
}