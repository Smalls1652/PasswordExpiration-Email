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

    $EmailedUsers = @()

    foreach ($User in $Users) {

        if (!($User.Expired)) {
            if (($DayIntervals -and ($Days -contains $User.PasswordExpiresIn)) -or !($DayIntervals)) {
                Write-Verbose "Emailing $($User.Email)..."
                $Subject = "Alert: Password Expiration Notice ($($User.PasswordExpiresIn.Days) days)"
                

                try {
                    $BodyHTMLSend = $BodyHTML.Replace("`$USERNAME", $User.Name).Replace("`$EXPIREINDAYS", $User.PasswordExpiresIn.Days).Replace("`$EXPIREDATE", (Get-Date $User.PasswordExpiration -Format g))

                    if ($PSCmdlet.ShouldProcess($User.Email, "Send Email")) {

                        Send-GraphMailClientMessage -FromAddress $EmailAddress -ToAddress $User.Email -Subject $Subject -Body $BodyHTMLSend -BodyType "HTML" -Attachments $Attachments
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
        [string]$BodyType,
        [Parameter(Position = 6)]
        [System.IO.FileInfo[]]$Attachments
    )

    process {
       
        #Get the HttpClient object from the Microsoft.Graph module
        $graphClient = [Microsoft.Graph.PowerShell.Authentication.Helpers.HttpHelpers]::GetGraphHttpClient($graphClient)

        #Use the 'MicrosoftGraphMessage' class as a base for generating the message.
        $graphMessageObj = [Microsoft.Graph.PowerShell.Models.MicrosoftGraphMessage]@{
            "Subject"         = $Subject;
            "BodyContent"     = $Body;
            "BodyContentType" = $BodyType;
            "HasAttachments"  = $false;
        }

        #Add each address in ToAddress to a list of 'MicrosoftGraphRecipient'.
        $toAddressList = [System.Collections.Generic.List[Microsoft.Graph.PowerShell.Models.MicrosoftGraphRecipient]]::new()
        foreach ($address in $ToAddress) {
            $toAddressList.Add([Microsoft.Graph.PowerShell.Models.MicrosoftGraphRecipient]@{
                    "EmailAddress" = [Microsoft.Graph.PowerShell.Models.MicrosoftGraphEmailAddress]@{
                        "Address" = $address
                    }
                }
            )
        }
        $graphMessageObj.ToRecipients = $toAddressList

        #Add each address in CcAddress to a list of 'MicrosoftGraphRecipient', if input is provided.
        switch ($null -eq $CcAddress) {
            $false {
                $ccAddressList = [System.Collections.Generic.List[Microsoft.Graph.PowerShell.Models.MicrosoftGraphRecipient]]::new()
                foreach ($address in $CcAddress) {
                    $ccAddressList.Add([Microsoft.Graph.PowerShell.Models.MicrosoftGraphRecipient]@{
                            "EmailAddress" = [Microsoft.Graph.PowerShell.Models.MicrosoftGraphEmailAddress]@{
                                "Address" = $address
                            }
                        }
                    )
                }

                $graphMessageObj.CcRecipients = $ccAddressList
                break
            }
        }

        #Convert the base 'MicrosoftGraphMessage' object to a JSON and then convert it into a hashtable.
        #There is a strange quirk with the available 'MicrosoftGraphAttachment' class that does not include options for either 'contentBytes' or '@odata.type'. To make it easier to add attachments to the mailMessage variable, we're converting it to a hashtable.
        $mailMessage = $graphMessageObj.ToJsonString() | ConvertFrom-Json -AsHashtable

        switch ($null -eq $Attachments) {
            $false {
                $attachmentsList = [System.Collections.Generic.List[hashtable]]::new()
                foreach ($attachment in $Attachments) {
                    $attachmentInBase64 = [System.Convert]::ToBase64String((Get-Content -Path $attachment.FullName -Raw -AsByteStream))

                    $attachmentsList.Add(
                        @{
                            "@odata.type"  = "#microsoft.graph.fileAttachment";
                            "name"         = $attachment.Name;
                            "contentBytes" = $attachmentInBase64;
                            "isInline"     = $true;
                        }
                    )
                }

                $mailMessage['hasAttachments'] = $true;
                $mailMessage.Add("attachments", $attachmentsList)
                break
            }
        }

        <#
        
        $postBody is formatted the way it is by what is stated in the '/users/{Id | UserPrincipalName}/sendMail' documentation for what is needed in the request body:
        https://docs.microsoft.com/en-us/graph/api/user-sendmail?view=graph-rest-1.0&tabs=http#request-body

        #>
        $postBody = @{
            "message"         = $mailMessage;
            "saveToSentItems" = $false
        }

        #Convert $postBody to a 'System.Net.Http.StringContent' class and ensure the header property, 'ContentType', is set to 'application/json'.
        $postBodyStringContent = [System.Net.Http.StringContent]::new(($postBody | ConvertTo-Json -Depth 4))
        $postBodyStringContent.Headers.ContentType = "application/json"

        #Build the full request Uri.
        $requestUri = "users/$($FromAddress)/sendMail"
        $fullUri = [System.Uri]::new($graphClient.BaseAddress, $requestUri)

        #Build the 'System.Net.Http.HttpRequestMessage' class to pass through the HttpClient.
        $httpRequestMessage = [System.Net.Http.HttpRequestMessage]@{
            "Content" = $postBodyStringContent;
            "RequestUri" = $fullUri;
            "Method" = ([System.Net.Http.HttpMethod]::Post);
        }

        #Send the data to the GraphAPI
        $apiResponse = $graphClient.SendAsync($httpRequestMessage).GetAwaiter().GetResult()

        return $apiResponse
    }
}