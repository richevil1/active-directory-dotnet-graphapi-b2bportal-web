$startTime=Get-Date
Import-Module Azure -ErrorAction SilentlyContinue

#DEPLOYMENT OPTIONS
    #Github source branch
    $Branch                  = "master"

    #optional, defines uniqueness for deployments
    $TestNo                  = "1"
    #region to deploy into - see https://azure.microsoft.com/en-us/regions/
    $DeployRegion            = "West US 2"
    #Name of your company - will be displayed through your site
    $CompanyName             = "Contoso"

    #The name of the Azure AD tenant that will host your two auth apps
    $TenantName              = "contoso.com"
    #The GUID of that tenant
    $AADTenantId             = "[AAD TenantID for auth app hosting]"
    #The name of your Azure subscription associated with your Azure AD auth tenant
    $AADSubName              = "ADTestTenant"

    #The GUID of your Azure AD tenant that's associated with your Azure subscription (where the site will be deployed)
    $AzureTenantId           = "[AAD TenantID for web app hosting]"
    #The name of that subscription
    $AzureSubName            = "MyAzureSubscription"

    #The name of the Resource Group where all of these resources will be deployed
    $RGName                  = "B2BTest$TestNo"
    #The "name" of your web application
    $SiteName                = "B2BDeployTest$TestNo"

    #The display name of your Azure AD "pre-auth" auth app. This is the app prospective guests will optionally use to prove their
    #identity via their home account
    $PreauthAppName          = "$CompanyName - B2B Pre-Authentication Sign-In$TestNo"
    #A unique URI that defines your application. Unlike the admin URI, this one must be unique in the world, as it's a multi-tenant application
    $PreAuthAppUri           = "https://$($SiteName).$TenantName"

    #The display name of your Azure AD administrative auth app. This name is displayed when a user logs in to your app from Azure AD
    $AdminAppName            = "$CompanyName - B2B Self-Serve Administration$TestNo"
    #A unique URI that defines your application
    $AdminAppUri             = "$($PreAuthAppUri)/b2badmin"

    #generating a unique "secret" for your admin app to execute B2B operations on your behalf
	$bytes = New-Object Byte[] 32
	$rand = [System.Security.Cryptography.RandomNumberGenerator]::Create()
	$rand.GetBytes($bytes)
	$rand.Dispose()
	$spAdminPassword = [System.Convert]::ToBase64String($bytes)
    $spSecAdminPassword = ConvertTo-SecureString $spAdminPassword -AsPlainText -Force -ErrorAction Stop

#END DEPLOYMENT OPTIONS

#Dot-sourced variable override (optional, comment out if not using)
if (Test-Path "$($env:PSH_Settings_Files)B2BPortal.ps1") {
    . "$($env:PSH_Settings_Files)B2BPortal.ps1"
}
#ensure we're logged in
try {
    $ctx=Get-AzureRmContext -ErrorAction Stop
}
catch {
    Login-AzureRmAccount -SubscriptionName $AADSubName -TenantId $AADTenantId -ErrorAction Stop
}

#this will only work if the same account can see the tenant and Azure sub at the same time
$ctx = Set-AzureRmContext -TenantId $AADTenantId -SubscriptionName $AADSubName -ErrorAction Stop
$cacheItems = $ctx.TokenCache.ReadItems()
$token = ($cacheItems | where { $_.Resource -eq "https://graph.windows.net/" })
if ($token.GetType().Name.Equals("Object[]")) {
    $token = $token.Item($token.Count-1)
}
if ($token.ExpiresOn -le [System.DateTime]::UtcNow) {
    $ac = [Microsoft.IdentityModel.Clients.ActiveDirectory.AuthenticationContext]::new("$($ctx.Environment.ActiveDirectoryAuthority)$($ctx.Tenant.Id)",$token)
    $token = $ac.AcquireTokenByRefreshToken($token.RefreshToken, "1950a258-227b-4e31-a9cf-717495945fc2", "https://graph.windows.net")
}
$aad = Connect-AzureAD -AadAccessToken $token.AccessToken -AccountId $ctx.Account.Id -TenantId $ctx.Tenant.Id

$newApps = $false;

$adminApp = Get-AzureRmADApplication -DisplayNameStartWith $AdminAppName -ErrorAction Stop
if ($adminApp -eq $null) {
    #generate required AzureAD applications
    #note: setting loopback on apps for now - will update after the ARM deployment is complete (below)...
    $adminAppReq = [System.Collections.Generic.List[Microsoft.Open.AzureAD.Model.RequiredResourceAccess]]::new()
    $ResourceColl = [System.Collections.Generic.List[Microsoft.Open.AzureAD.Model.ResourceAccess]]::new()

    #MSGraph
    
        $MSGraphAppDirRWAll = [Microsoft.Open.AzureAD.Model.ResourceAccess]::new("19dbc75e-c2e2-444c-a770-ec69d8559fc7","Role")       # MSGraph App-Directory.ReadWrite.All
        $ResourceColl.Add($MSGraphAppDirRWAll)

        $MSGraphAppUserRWAll = [Microsoft.Open.AzureAD.Model.ResourceAccess]::new("741f803b-c850-494e-b5df-cde7c675a1ca","Role")      # MSGraph App-User.ReadWrite.All
        $ResourceColl.Add($MSGraphAppUserRWAll)

        $MSGraphDelUserInviteAll = [Microsoft.Open.AzureAD.Model.ResourceAccess]::new("63dd7cd9-b489-4adf-a28c-ac38b9a0f962","Scope") # MSGraph Delegated-User.Invite.All
        $ResourceColl.Add($MSGraphDelUserInviteAll)
        $MSGraph = [Microsoft.Open.AzureAD.Model.RequiredResourceAccess]::new("00000003-0000-0000-c000-000000000000",$ResourceColl)   # MSGraph resources

    $adminAppReq.Add($MSGraph)

    $ResourceColl.Clear()
    #AADGraph
        $AADGraphDelUserRead = [Microsoft.Open.AzureAD.Model.ResourceAccess]::new("311a71cc-e848-46a1-bdf8-97ff7156d8e6","Scope")     # AADGraph Delegated-User.Read
        $ResourceColl.Add($AADGraphDelUserRead)
        $AADGraph = [Microsoft.Open.AzureAD.Model.RequiredResourceAccess]::new("00000002-0000-0000-c000-000000000000",$ResourceColl)  # AADGraph resources

    $adminAppReq.Add($AADGraph)

    $adminApp = AzureAD\New-AzureADApplication `
        -DisplayName $AdminAppName `
        -IdentifierUris $AdminAppUri `
        -RequiredResourceAccess $adminAppReq `
        -ErrorAction Stop

    #$adminApp = New-AzureRmADApplication -DisplayName $AdminAppName -HomePage "https://loopback" -IdentifierUris $AdminAppUri
    
    New-AzureRmADServicePrincipal -ApplicationId $adminApp.AppId
    $newApps = $true
}

$preauthApp = Get-AzureRmADApplication -DisplayNameStartWith $PreAuthAppName
if ($preauthApp -eq $null) {

    $preauthAppReq = [System.Collections.Generic.List[Microsoft.Open.AzureAD.Model.RequiredResourceAccess]]::new()
    $ResourceColl = [System.Collections.Generic.List[Microsoft.Open.AzureAD.Model.ResourceAccess]]::new()

    #AADGraph
        $AADGraphDelUserRead = [Microsoft.Open.AzureAD.Model.ResourceAccess]::new("311a71cc-e848-46a1-bdf8-97ff7156d8e6","Scope")     # AADGraph Delegated-User.Read
        $ResourceColl.Add($AADGraphDelUserRead)
        $AADGraph = [Microsoft.Open.AzureAD.Model.RequiredResourceAccess]::new("00000002-0000-0000-c000-000000000000",$ResourceColl)  # AADGraph resources

    $preauthAppReq.Add($AADGraph)

    $preauthApp = AzureAD\New-AzureADApplication `
        -DisplayName $PreAuthAppName `
        -IdentifierUris $PreAuthAppUri `
        -RequiredResourceAccess $preauthAppReq `
        -ErrorAction Stop

    #$preauthApp = New-AzureRmADApplication -DisplayName $PreauthAppName -HomePage "https://loopback" -IdentifierUris $PreauthAppUri -AvailableToOtherTenants $true
    
    New-AzureRmADServicePrincipal -ApplicationId $preauthApp.AppId
    $newApps = $true
}

if ($newApps) {
    Start-Sleep 15
}

$adminAppCred = Get-AzureRmADAppCredential -ApplicationId $adminApp.AppId
if ($adminAppCred -eq $null) {
    New-AzureRmADAppCredential -ApplicationId $adminApp.AppId -Password $spSecAdminPassword
}
$preauthAppCred = Get-AzureRmADAppCredential -ApplicationId $preauthApp.AppId
if ($preauthAppCred -eq $null) {
    New-AzureRmADAppCredential -ApplicationId $preauthApp.AppId -Password $spSecAdminPassword
}

#deploy
Set-AzureRmContext -SubscriptionName $AzureSubName -TenantId $AzureTenantId -ErrorAction Stop

$parms=@{
    "hostingPlanName"             = $SiteName;
    "skuName"                     = "F1";
    "skuCapacity"                 = 1;
    "tenantDomainName"            = $TenantName;
    "tenantId"                    = $AADTenantId;
    "clientId_admin"              = $adminApp.AppId;
    "clientSecret_admin"          = $spAdminPassword;
    "clientId_preAuth"            = $preauthApp.AppId;
    "clientSecret_preAuth"        = $spAdminPassword;
    "mailServerFqdn"              = "";
    "smtpLogin"                   = "";
    "smptPassword"                = "";
    "branch"                      = $Branch;
}

$TemplateFile = "https://raw.githubusercontent.com/Azure/active-directory-dotnet-graphapi-b2bportal-web/$Branch/azuredeploy.json"

try {
    Get-AzureRmResourceGroup -Name $RGName -ErrorAction Stop
    Write-Host "Resource group $RGName exists, updating deployment"
}
catch {
    $RG = New-AzureRmResourceGroup -Name $RGName -Location $DeployRegion
    Write-Host "Created new resource group $RGName."
}
$version ++
$deployment = New-AzureRmResourceGroupDeployment -ResourceGroupName $RGName -TemplateParameterObject $parms -TemplateFile $TemplateFile -Name "B2BDeploy$version" -Force -Verbose

if ($deployment.ProvisioningState -ne "Failed") {
    #to-do: update URIs and reply URLs for apps, based on output parms from $deployment
    #also to-do: update application permissions and APIs - may need to be done in the portal
    $hostName = $Deployment.Outputs.webSiteObject.Value.enabledHostNames.Item(0).ToString()
    $url = "https://$hostname/"
    Set-AzureADApplication -ObjectId $adminApp.ObjectId -ReplyUrls @($Url) -Homepage $url
    Set-AzureADApplication -ObjectId $preauthApp.ObjectId -ReplyUrls @($Url) -Homepage $url

    $ProjectFolder = "$env:USERPROFILE\desktop\$RGName\"
    if (!(Test-Path -Path $ProjectFolder)) {
        md $ProjectFolder
    }
    $WshShell = New-Object -comObject WScript.Shell
    $Shortcut = $WshShell.CreateShortcut("$($ProjectFolder)B2B Self-Service Site.lnk")
    $Shortcut.TargetPath = $url
    $Shortcut.IconLocation = "%ProgramFiles%\Internet Explorer\iexplore.exe, 0"
    $Shortcut.Save()
    start $ProjectFolder
}

$endTime=Get-Date

Write-Host ""
Write-Host "Total Deployment time:"
New-TimeSpan -Start $startTime -End $endTime | Select Hours, Minutes, Seconds
