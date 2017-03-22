# Azure Active Directory/ASP.Net MVC/GraphAPI B2BPortal
## Sample/Prototype project enabling self-service B2B capabilities for an Azure AD Tenant
## Quick Start
<img src="http://azuredeploy.net/deploybutton.png"/>

(coming soon)


__Details__
* Allows self-service provisioning of guest accounts in a tenant. Portal enables this via API calls to the Microsoft Graph
* Deploys the following:
  * Azure Web App
  * Azure DocumentDB
* Requires the following:
  * Azure AD application with the following:
    * Microsoft Graph - app permissions
      * Read and write directory data
      * Read and write all users' full profiles
    * Microsoft Graph - delegated permissions
      * Sign in and read user profile
      * Read and write access to user profile
      * Read directory data
      * Read and write directory data
  * Optional - custom DNS name and SSL cert

__Operation__

* Guest users access the home page and may enter their login email to request access to the host tenant/company. Optionally, they may click to "Pre-Auth" - this will allow them to login to the guest's home tenant, authenticate, then return with the form pre-filled AND with the request authenticated and validated.
* Once the request is submitted, the request will be queued in a DocumentDB repo.
* A user in the home company with the "Guest Submitter" role granted ( can then access the portal, log in, and browse the pending requests, either approving, denying, or leaving in a pending state for others to review. Additionally, internal comments can be attached to the request records.
* Optionally, an admin may login and add a "Pre-Auth" domain record. This will allow all Pre-Authed users with a matching domain suffix, to be automatically approved for B2B guest access in the tenant. 
* Whether a user is automatically approved, or manually approved, once an approval occurs, a welcome email is generated to the requester with a link that allows for redemption of the request. 

# Contributing

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/). For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.
