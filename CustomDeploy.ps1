$startTime=Get-Date
Import-Module Azure -ErrorAction SilentlyContinue

#DEPLOYMENT OPTIONS
    $CompanyName             = "HackerDemo"
    $TenantId                = "a70fadca-e867-489c-b119-72dc9f00c26b"
    $RGName                  = "<YOUR RESOURCE GROUP>"
    $DeployRegion            = "<SELECT AZURE REGION>"
    $SiteName                = "B2BDeployTest1"
    $AdminAppName            = "B2B Self-Serve Administration"
    $PreauthAppName          = "$CompanyName - B2B Pre-Authentication Sign-In"
    $pw1                     = [Guid]::NewGuid().ToString().Replace("-","")
    $pw2                     = [System.Text.Encoding]::UTF8.GetBytes($pw1)
    $spAdminPassword         = [System.Convert]::ToBase64String($pw2)
#END DEPLOYMENT OPTIONS

#Login if necessary
$AzureSub="GTP - Brett Hacker"
#$AzureSub="MSDN"

try {
    $ctx=Get-AzureRmContext -ErrorAction Stop
}
catch {
    Login-AzureRmAccount
}
#this will only work if the same account can see the tenant and Azure sub at the same time
Set-AzureRmContext -SubscriptionName "ADTenantTest" -TenantId $TenantId

#Dot-sourced variable override (optional, comment out if not using)
#. C:\dev\A_CustomDeploySettings\B2BPortal.ps1

#ensure we're logged in
Get-AzureRmContext -ErrorAction Stop

#generate required AzureAD applications
#note: setting loopback on apps for now - will update after the ARM deployment is complete (below)...
$adminApp = New-AzureRmADApplication -DisplayName $AdminAppName -HomePage "https://loopback" -IdentifierUris "https://$($SiteName)admin"
    New-AzureRmADServicePrincipal -ApplicationId $adminApp.ApplicationId
$preauthApp = New-AzureRmADApplication -DisplayName $PreauthAppName -HomePage "https://$($SiteName).azurewebsites.net" -IdentifierUris "https://$($SiteName).hackerdemo.com" -AvailableToOtherTenants $true
    New-AzureRmADServicePrincipal -ApplicationId $preauthApp.ApplicationId

Start-Sleep 15
New-AzureRmRoleAssignment -RoleDefinitionName Reader -ServicePrincipalName $adminApp.ApplicationId
New-AzureRmRoleAssignment -RoleDefinitionName Reader -ServicePrincipalName $preauthApp.ApplicationId

$spSecurePw =  ConvertTo-SecureString $spAdminPassword -AsPlainText -Force -ErrorAction Stop
[PSCredential]$adminAppCreds = New-Object PSCredential ($adminApp.ApplicationId, $spSecurePw)

New-AzureRmADAppCredential -ApplicationId $adminApp.ApplicationId -Password $spAdminPassword

#deploy
if ($ctx.SubscriptionName -ne $AzureSub) {
    Set-AzureRmContext -SubscriptionName $AzureSub
}

$parms=@{
    "hostingPlanName"             = $SiteName;
    "skuName"                     = "F1";
    "skuCapacity"                 = 1;
    "tenantName"                  = $AdfsFarmCount;
    "clientId_admin"              = $usersArray;
    "clientSecret_admin"          = "";
    "clientId_preAuth"            = "";
    "mailServerFqdn"              = "";
    "smtpLogin"                   = "";
    "smptPassword"                = "";
}

$TemplateFile = "https://raw.githubusercontent.com/Azure/active-directory-dotnet-graphapi-b2bportal-web/master/azuredeploy.json"

try {
    Get-AzureRmResourceGroup -Name $RGName -ErrorAction Stop
    Write-Host "Resource group $RGName exists, updating deployment"
}
catch {
    $RG = New-AzureRmResourceGroup -Name $RGName -Location $DeployRegion -Tag @{ Shutdown = "true"; Startup = "false"}
    Write-Host "Created new resource group $RGName."
}
$version ++
$deployment = New-AzureRmResourceGroupDeployment -ResourceGroupName $RGName -TemplateParameterObject $parms -TemplateFile $TemplateFile -Name "adfsDeploy$version"  -Force -Verbose

if ($deployment) {
    if (-not (Get-Command Get-FQDNForVM -ErrorAction SilentlyContinue)) {
        #load add-on functions to facilitate the RDP connectoid creation below
        $url="$($assetLocation)Scripts/Addons.ps1"
        $tempfile = "$env:TEMP\Addons.ps1"
        $webclient = New-Object System.Net.WebClient
        $webclient.DownloadFile($url, $tempfile)
        . $tempfile
    }

    $RDPFolder = "$env:USERPROFILE\desktop\$RGName\"
    if (!(Test-Path -Path $RDPFolder)) {
        md $RDPFolder
    }
    $ADName = $ADDomainName.Split('.')[0]
    $vms = Find-AzureRmResource -ResourceGroupNameContains $RGName | where {($_.ResourceType -like "Microsoft.Compute/virtualMachines")}
    $pxcount=0
    if ($vms) {
        foreach ($vm in $vms) {
            $fqdn=Get-FQDNForVM -ResourceGroupName $RGName -VMName $vm.Name
            New-RDPConnectoid -ServerName $fqdn -LoginName "$($ADName)\$($userName)" -RDPName $vm.Name -OutputDirectory $RDPFolder -Width $RDPWidth -Height $RDPHeight
            if ($vm.Name.IndexOf("PX") -gt -1) {
                $pxcount++
                $WshShell = New-Object -comObject WScript.Shell
                $Shortcut = $WshShell.CreateShortcut("$($RDPFolder)ADFSTest$pxcount.lnk")
                $Shortcut.TargetPath = "https://$fqdn/adfs/ls/idpinitiatedsignon.aspx"
                $Shortcut.IconLocation = "%ProgramFiles%\Internet Explorer\iexplore.exe, 0"
                $Shortcut.Save()
            }
        }
    }

    start $RDPFolder
}

$endTime=Get-Date

Write-Host ""
Write-Host "Total Deployment time:"
New-TimeSpan -Start $startTime -End $endTime | Select Hours, Minutes, Seconds
