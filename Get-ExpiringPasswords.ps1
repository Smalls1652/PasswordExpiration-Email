function Get-ExpiringPasswords {
    param(
        [int]$DaysRemaining = 10,
        [int]$MaxAlive = 120,
        [Parameter(Mandatory = $true)]$OUPath
    )


    Write-Progress -Id 1 -Activity "Grabbing Data" -Status "Progress" -CurrentOperation "Grabbing accounts from AD..."
    try {
        $Users = Get-ADUser -Filter "Enabled -eq 'true'" -SearchBase $OUPath -Properties "PasswordLastSet", "EmailAddress", "LastLogonDate" -ErrorAction Stop | Where-Object { $_."LastLogonDate" -gt (Get-Date).AddYears(-1) -and $_."EmailAddress" -ne $null }
    }
    catch {
        Write-Error "There was an error grabbing data from Active Directory."
    }

    Write-Progress -Id 1 -Activity "Grabbing Data" -Completed


    $i = 1
    $z = 0
    $totalUsers = $Users | Measure-Object | Select-Object -ExpandProperty "Count"

    $ExpiringUsers = @()
    foreach ($User in $Users) {
        Write-Progress -Id 2 -Activity "Parsing Data" -Status "Progress: [$($i)/$($totalUsers)] - Found: [$($z)]" -CurrentOperation "Parsing data for $($User.Name)..." -PercentComplete ($i / $totalUsers * 100)
        $PasswordExpirationDate = $User.PasswordLastSet.AddDays($MaxAlive)
        $PasswordLife = ((Get-Date) - ($User.PasswordLastSet)) | Select-Object -ExpandProperty "Days"
        $PasswordExpiresIn = ($PasswordExpirationDate - (Get-Date)) | Select-Object -ExpandProperty "Days"

        if ($PasswordExpiresIn -lt 0) {
            $PasswordExpiresIn = 0
        }
        

        $defaultOutput = "SamAccountName", "Email", "PasswordExpiration", "Expires In", "Expired"
        $defaultPropertySet = New-Object System.Management.Automation.PSPropertySet("DefaultDisplayPropertySet", [string[]]$defaultOutput)
        $CustomOutput = [System.Management.Automation.PSMemberInfo[]]@($defaultPropertySet)

        $o = $null
        if ($PasswordLife -ge $MaxAlive) {
            $o = New-Object -TypeName psobject -Property @{
                "SamAccountName"      = $User.SamAccountName;
                "Email"               = $User.EmailAddress;
                "Name"                = $User.Name;
                "PasswordLastSet"     = $User.PasswordLastSet;
                "PasswordExpiration"  = $PasswordExpirationDate;
                "PasswordLife (Days)" = "$($PasswordLife) days";
                "Expires In"          = "$($PasswordExpiresIn) days";
                "PasswordLife"        = $PasswordLife;
                "PasswordExpiresIn"   = $PasswordExpiresIn;
                "Expired"             = $true
            }
            Add-Member -InputObject $o -MemberType MemberSet -Name PSStandardMembers -Value $CustomOutput
            #$ExpiringUsers += 
        }
        elseif ($PasswordExpiresIn -le $DaysRemaining) {
            $o = New-Object -TypeName psobject -Property @{
                "SamAccountName"      = $User.SamAccountName;
                "Email"               = $User.EmailAddress;
                "Name"                = "$($User.GivenName) $($User.Surname)";
                "PasswordLastSet"     = $User.PasswordLastSet;
                "PasswordExpiration"  = $PasswordExpirationDate;
                "PasswordLife (Days)" = "$($PasswordLife) days";
                "Expires In"          = "$($PasswordExpiresIn) days";
                "PasswordLife"        = $PasswordLife;
                "PasswordExpiresIn"   = $PasswordExpiresIn;
                "Expired"             = $false
            }
            Add-Member -InputObject $o -MemberType MemberSet -Name PSStandardMembers -Value $CustomOutput
            #$ExpiringUsers += 
        }

        if ($o) {
            $z++
            $ExpiringUsers += $o
        }

        $i++
    }
    Write-Progress -Id 2 -Activity "Parsing Data" -Completed
    return $ExpiringUsers
}

function New-ExpirationEmail {
    [CmdletBinding(SupportsShouldProcess = $true)]
    param(
        [Parameter(Mandatory = $true)][psobject]$Users,
        [Parameter(Mandatory = $true)][string]$EmailAddress,
        [Parameter(Mandatory = $true)][pscredential]$EmailCredential,
        [Parameter(Mandatory = $true)][string]$BodyHTML,
        [Parameter(Mandatory = $true)][string]$SMTPServer,
        [Parameter(Mandatory = $true)][int]$SMTPPort,
        [switch]$DayIntervals,
        [array]$Days = @(10,5,2,1)
    )

    $EmailedUsers = @()

    foreach ($User in $Users) {

        if (!($User.Expired)) {
            if (($DayIntervals -and ($Days -contains $User.PasswordExpiresIn)) -or !($DayIntervals)) {
                Write-Verbose "Emailing $($User.Email)..."
                $Subject = "Alert: Password Expiration Notice ($($User.PasswordExpiresIn) days)"
                

                try {
                    if ($PSCmdlet.ShouldProcess($User.Email, "Send Email")) {
                        Send-MailMessage -To $User.Email -From $EmailAddress -Priority High -UseSsl -Credential $EmailCredential -SmtpServer $SMTPServer -Port $SMTPPort -Subject $Subject -BodyAsHtml -Body $BodyHTML -ErrorAction Continue
                    }
                    $EmailedUsers += $User
                }
                catch {
                    Write-Error "There was an error emailing $($User.Email)."
                }
            }
        }
    }
}