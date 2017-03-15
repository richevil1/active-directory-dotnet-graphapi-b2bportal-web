using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;
using System.Configuration;
using System.Globalization;
using System.Threading.Tasks;


namespace B2BPortal
{
    public partial class Startup
    {
        //
        // The Client ID is used by the application to uniquely identify itself to Azure AD.
        // The Metadata Address is used by the application to retrieve the signing keys used by Azure AD.
        // The AAD Instance is the instance of Azure, for example public Azure or Azure China.
        // The Authority is the sign-in URL of the tenant.
        // The Post Logout Redirect Uri is the URL where the user will be redirected after they sign out.
        //
        private static string clientId = ConfigurationManager.AppSettings["ida:ClientId"];
        private static string aadInstance = ConfigurationManager.AppSettings["ida:AADInstance"];
        private static string tenant = ConfigurationManager.AppSettings["ida:Tenant"];
        private static string postLogoutRedirectUri = ConfigurationManager.AppSettings["ida:PostLogoutRedirectUri"];

        string authority = String.Format(CultureInfo.InvariantCulture, aadInstance, tenant);

        public void ConfigureAuth(IAppBuilder app)
        {
            app.SetDefaultSignInAsAuthenticationType(CookieAuthenticationDefaults.AuthenticationType);

            app.UseCookieAuthentication(new CookieAuthenticationOptions());

            app.UseOpenIdConnectAuthentication(
                new OpenIdConnectAuthenticationOptions
                {
                    ClientId = clientId,
                    Authority = authority,
                    PostLogoutRedirectUri = postLogoutRedirectUri,
                    RedirectUri = postLogoutRedirectUri,
                    //"id_token token" ACCESS TOKENS NOT SUPPORTED HERE, REQUIRES S2S FLOW
                    //ResponseType = "id_token",
                    Notifications = new OpenIdConnectAuthenticationNotifications
                    {
                        AuthenticationFailed = context =>
                        {
                            context.HandleResponse();
                            context.Response.Redirect("/Error?message=" + context.Exception.Message);
                            return Task.FromResult(0);
                        }
/*                      THESE CALLBACKS HELPED CONVINCE ME THAT ACCESS TOKENS WOULD REQUIRE A SEPARATE AUTH PATH
                        SecurityTokenValidated = context =>
                        {
                            string accessToken = context.ProtocolMessage.AccessToken;
                            string IdToken = context.ProtocolMessage.IdToken;
                            string Code = context.ProtocolMessage.Code;
                            return Task.FromResult(0);
                        },
                        SecurityTokenReceived = context =>
                        {
                            string accessToken = context.ProtocolMessage.AccessToken;
                            string IdToken = context.ProtocolMessage.IdToken;
                            string Code = context.ProtocolMessage.Code;
                            return Task.FromResult(0);
                        }
*/
                    }
                });
        }
    }
}