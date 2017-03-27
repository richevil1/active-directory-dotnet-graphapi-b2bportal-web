$startTime=Get-Date
Import-Module Azure -ErrorAction SilentlyContinue

#DEPLOYMENT OPTIONS
    $TestNo                  = "3"
    $DeployRegion            = "West US 2"
    $CompanyName             = "Contoso"

    $TenantName              = "contoso.com"
    $AADTenantId             = "[AAD TenantID for auth app hosting]"
    $AADSubName              = "ADTestTenant"

    $AzureTenantId           = "[AAD TenantID for web app hosting]"
    $AzureSubName            = "MyAzureSubscription"

    $RGName                  = "B2BTest$TestNo"
    $SiteName                = "B2BDeployTest$TestNo"

    $AdminAppName            = "B2B Self-Serve Administration$TestNo"
    $AdminAppUri             = "https://$($SiteName)admin.$TenantName"
    $PreauthAppName          = "$CompanyName - B2B Pre-Authentication Sign-In$TestNo"
    $PreAuthAppUri           = "https://$($SiteName).$TenantName"

    $pw1                     = [Guid]::NewGuid().ToString().Replace("-","")
    $pw2                     = [System.Text.Encoding]::UTF8.GetBytes($pw1)
    $spAdminPassword         = [System.Convert]::ToBase64String($pw2)
#END DEPLOYMENT OPTIONS

#ensure we're logged in
try {
    $ctx=Get-AzureRmContext -ErrorAction Stop
}
catch {
    Login-AzureRmAccount -TenantId $AADTenantID -SubscriptionName $AADSubName
}

#Dot-sourced variable override (optional, comment out if not using)
. C:\dev\A_CustomDeploySettings\B2BPortal.ps1

#this will only work if the same account can see the tenant and Azure sub at the same time
Set-AzureRmContext -SubscriptionName $AADSubName -TenantId $AADTenantId

$newApps = $false;

$adminApp = Get-AzureRmADApplication -DisplayNameStartWith $AdminAppName
if ($adminApp -eq $null) {
    #generate required AzureAD applications
    #note: setting loopback on apps for now - will update after the ARM deployment is complete (below)...
    $adminApp = New-AzureRmADApplication -DisplayName $AdminAppName -HomePage "https://loopback" -IdentifierUris $AdminAppUri
        New-AzureRmADServicePrincipal -ApplicationId $adminApp.ApplicationId
    $newApps = $true
}

$preauthApp = Get-AzureRmADApplication -DisplayNameStartWith $PreAuthAppName
if ($preauthApp -eq $null) {
    $preauthApp = New-AzureRmADApplication -DisplayName $PreauthAppName -HomePage "https://$($SiteName).azurewebsites.net" -IdentifierUris $PreauthAppUri -AvailableToOtherTenants $true
        New-AzureRmADServicePrincipal -ApplicationId $preauthApp.ApplicationId
    $newApps = $true
}

if ($newApps) {
    Start-Sleep 15
}

$adminAppCred = Get-AzureRmADAppCredential -ApplicationId $adminApp.ApplicationId
if ($adminAppCred -eq $null) {
    New-AzureRmADAppCredential -ApplicationId $adminApp.ApplicationId -Password $spAdminPassword
}

#New-AzureRmRoleAssignment -RoleDefinitionName Reader -ServicePrincipalName $adminApp.ApplicationId
#New-AzureRmRoleAssignment -RoleDefinitionName Reader -ServicePrincipalName $preauthApp.ApplicationId


#deploy
if ($ctx.SubscriptionName -ne $AzureSub) {
    Set-AzureRmContext -SubscriptionName $AzureSubName -TenantId $AzureTenantId
}

$parms=@{
    "hostingPlanName"             = $SiteName;
    "skuName"                     = "F1";
    "skuCapacity"                 = 1;
    "tenantName"                  = $TenantName;
    "tenantId"                    = $TenantId;
    "clientId_admin"              = $adminApp.ApplicationId;
    "clientSecret_admin"          = $spAdminPassword;
    "clientId_preAuth"            = $preauthApp.ApplicationId;
    "mailServerFqdn"              = "";
    "smtpLogin"                   = "";
    "smptPassword"                = "";
}

#$TemplateFile = "https://raw.githubusercontent.com/Azure/active-directory-dotnet-graphapi-b2bportal-web/master/azuredeploy.json"
$TemplateFile = "C:\Dev\active-directory-dotnet-graphapi-b2bportal-web\azuredeploy.json"

try {
    Get-AzureRmResourceGroup -Name $RGName -ErrorAction Stop
    Write-Host "Resource group $RGName exists, updating deployment"
}
catch {
    $RG = New-AzureRmResourceGroup -Name $RGName -Location $DeployRegion
    Write-Host "Created new resource group $RGName."
}
$version ++
$deployment = New-AzureRmResourceGroupDeployment -ResourceGroupName $RGName -TemplateParameterObject $parms -TemplateFile $TemplateFile -Name "B2BDeploy$version"  -Force -Verbose

if ($deployment) {
    #to-do: update URIs and reply URLs for apps, based on output parms from $deployment

}

$endTime=Get-Date

Write-Host ""
Write-Host "Total Deployment time:"
New-TimeSpan -Start $startTime -End $endTime | Select Hours, Minutes, Seconds
