function Get-ExpiringPasswords {
    [CmdletBinding()]
    param(
        [Parameter(Position = 0)]
        [int]$DaysRemaining = 10,
        [Parameter(Position = 1)]
        [int]$MaxAlive = 120,
        [Parameter(Position = 2, Mandatory)]
        [string]$OUPath,
        [Parameter(Position = 3, Mandatory)]
        [string]$DomainName
    )

    begin {
        filter LastLogonandNotNullEmail {
            if (($PSItem.LastLogonDate -gt (Get-Date).AddYears(-1)) -and ($PSItem.UserPrincipalName -like "*$($accountSearcher.domainController.Domain)")) {
                $PSItem
            }
        }
    }

    process {

        $ProgressSplat = @{
            "Activity" = "Get user's with expiring passwords";
            "Status"   = "Progress->";
            "Id"       = 0;
        }
        
        $ProgressSplat2 = @{
            "Activity" = "Parsing user data";
            "Status"   = "Progress->";
            "Id"       = 1;
            "ParentId" = 0;
        }

        Write-Progress @ProgressSplat -PercentComplete 0 -CurrentOperation "Getting users from AD"


        $accountSearcher = [PasswordExpiration.Helpers.ActiveDirectory.AccountSearcher]::new($DomainName, $OUPath, $true)

        $users = $accountSearcher.GetUsers() | LastLogonandNotNullEmail

        $i = 1
        $totalUsers = ($users | Measure-Object).Count

        Write-Progress @ProgressSplat -PercentComplete 50 -CurrentOperation "Parsing users"
        $parser = [PasswordExpiration.Helpers.UserParser]::new()
        $parsedUsers = [System.Collections.Generic.List[PasswordExpiration.Classes.ParsedUserData]]::new()

        Write-Progress @ProgressSplat2 -PercentComplete 0 -CurrentOperation "Starting parse operations"
        foreach ($user in $users) {
            Write-Progress @ProgressSplat2 -CurrentOperation "Parsing data for $($user.UserName)..." -PercentComplete ($i / $totalUsers * 100)

            $p = $parser.ParseData($user, $MaxAlive, $DaysRemaining)
            $parsedUsers.Add($p)

            $i++
        }
        Write-Progress @ProgressSplat2 -Completed
        Write-Progress @ProgressSplat -Completed
    }
    
    end {
        return $parsedUsers
    }
}

function New-ExpirationEmail {
    [CmdletBinding(SupportsShouldProcess)]
    param(
        [Parameter(Position = 0, Mandatory)]
        [System.Collections.Generic.List[PasswordExpiration.Classes.ParsedUserData]]$Users,
        [Parameter(Position = 1, Mandatory)]
        [string]$EmailAddress,
        [Parameter(Position = 2, Mandatory)]
        [string]$BodyHTML,
        [Parameter(Position = 3)]
        [System.IO.FileInfo[]]$Attachments,
        [Parameter(Position = 4)]
        [switch]$DayIntervals,
        [Parameter(Position = 5)]
        [array]$Days = @(10, 5, 2, 1)
    )

    foreach ($User in $Users) {
        switch ($User.Expired) {
            $true {
                Write-Verbose "'$($User.UserPrincipalName)' - Password already expired."
                break
            }

            Default {
                switch ( (($DayIntervals -eq $true) -and ($User.PasswordExpiresIn.Days -in $Days)) -or ($DayIntervals -eq $false) ) {
                    $false {
                        break
                    }

                    Default {
                        Write-Verbose "Emailing $($User.UserPrincipalName)..."
                        $Subject = "Alert: Password Expiration Notice ($($User.PasswordExpiresIn.Days) days)"
                
                        try {
                            $BodyHTMLSend = $BodyHTML.Replace("`$USERNAME", $User.DisplayName).Replace("`$EXPIREINDAYS", $User.PasswordExpiresIn.Days).Replace("`$EXPIREDATE", (Get-Date $User.PasswordExpiration -Format g))

                            if ($PSCmdlet.ShouldProcess($User.UserPrincipalName, "Send Email")) {

                                $mailMessageObj = New-GraphMailClientMessage -ToAddress $User.UserPrincipalName -Subject $Subject -Body $BodyHTMLSend -BodyType "HTML" -Attachments $Attachments
                                Send-GraphMailClientMessage -MailMessage $mailMessageObj -FromAddress $EmailAddress
                            }

                            [PasswordExpiration.Classes.EmailSentStatus]@{
                                "UserPrincipalName" = $User.UserPrincipalName;
                                "EmailSent"         = $true
                            }
                        }
                        catch {
                            Write-Error "There was an error emailing $($User.UserPrincipalName)."
                            [PasswordExpiration.Classes.EmailSentStatus]@{
                                "UserPrincipalName" = $User.UserPrincipalName;
                                "EmailSent"         = $false
                            }
                        }
                        break
                    }
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

function New-GraphMailClientMessage {
    [CmdletBinding()]
    param(
        [Parameter(Position = 0, Mandatory)]
        [ValidateNotNullOrEmpty()]
        [string[]]$ToAddress,
        [Parameter(Position = 1)]
        [string[]]$CcAddress,
        [Parameter(Position = 2, Mandatory)]
        [ValidateNotNullOrEmpty()]
        [string]$Subject,
        [Parameter(Position = 3, Mandatory)]
        [ValidateNotNullOrEmpty()]
        [string]$Body,
        [Parameter(Position = 4, Mandatory)]
        [ValidateNotNullOrEmpty()]
        [string]$BodyType,
        [Parameter(Position = 5)]
        [System.IO.FileInfo[]]$Attachments
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
                    "subject"        = $Subject;
                    "body"           = (
                        [graph_email.Mail.Classes.MessageBody]@{
                            "contentType" = $BodyType;
                            "content"     = $Body;
                        }
                    );
                    "toRecipients"   = $toAddressList;
                    "ccRecipients"   = $ccAddressList;
                    "attachments"    = $null;
                    "hasAttachments" = $false;
                }
            );
            "saveToSentItems" = $false;
        }

        switch ($null -eq $Attachments) {
            $false {
                $attachmentsList = [System.Collections.Generic.List[graph_email.Mail.Classes.FileAttachment]]::new()
                foreach ($attachment in $Attachments) {
                    $attachmentRaw = $null

                    switch ($PSVersionTable.PSVersion.ToString() -like "5.1*") {
                        $true {
                            $attachmentRaw = Get-Content -Path $attachment.FullName -Raw -Encoding Byte
                            break
                        }

                        Default {
                            $attachmentRaw = Get-Content -Path $attachment.FullName -Raw -AsByteStream
                            break
                        }
                    }
                    $attachmentInBase64 = [System.Convert]::ToBase64String($attachmentRaw)

                    $attachmentsList.Add(
                        [graph_email.Mail.Classes.FileAttachment]@{
                            "name"         = $attachment.Name;
                            "contentBytes" = $attachmentInBase64;
                            "isInline"     = $true;
                        }
                    )
                }

                $mailMessage.message.hasAttachments = $true;
                $mailMessage.message.attachments = $attachmentsList
                break
            }
        }

        return $mailMessage
    }
}

function Send-GraphMailClientMessage {
    [CmdletBinding()]
    param(
        [Parameter(Position = 0, Mandatory)]
        [ValidateNotNullOrEmpty()]
        [graph_email.Mail.MailMessage]$MailMessage,
        [Parameter(Position = 1, Mandatory)]
        [ValidateNotNullOrEmpty()]
        [string]$FromAddress
    )

    process {

        $mailMessageJson = [graph_email.Mail.Classes.Conversion]::new().ToJson($MailMessage)

        $authToken = $PSCmdlet.SessionState.PSVariable.GetValue("GraphEmailClientToken")

        $restApiHeaders = @{
            "Authorization" = $authToken.CreateAuthorizationHeader();
            "Content-Type"  = "application/json"
        }

        Invoke-RestMethod -Method "Post" -Headers $restApiHeaders -Uri "https://graph.microsoft.com/v1.0/users/$($FromAddress)/sendMail" -Body $mailMessageJson -ErrorAction Stop
    }
}