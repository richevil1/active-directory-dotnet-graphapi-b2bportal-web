using Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;
using System.Threading.Tasks;
using Microsoft.Owin.Security.Notifications;
using Microsoft.IdentityModel.Protocols;
using System.Configuration;
using System.IdentityModel.Tokens;
using B2BPortal.Infrastructure;
using System;

namespace B2BPortal
{
    public partial class Startup
    {
        private const string discoverySuffix = "/.well-known/openid-configuration";
        public const string AcrClaimType = "http://schemas.microsoft.com/claims/authnclassreference";

        // App config settings
        private static string clientId_admin = ConfigurationManager.AppSettings["ida:ClientId_Admin"];
        private static string clientId_preAuth = ConfigurationManager.AppSettings["ida:ClientId_PreAuth"];

        private static string tenant = ConfigurationManager.AppSettings["ida:Tenant"];
        private static string aadInstanceMulti = ConfigurationManager.AppSettings["ida:AadInstanceMulti"];
        private static string aadInstanceLocal = ConfigurationManager.AppSettings["ida:AadInstanceLocal"];

        public void ConfigureAuth(IAppBuilder app)
        {
            app.SetDefaultSignInAsAuthenticationType(CookieAuthenticationDefaults.AuthenticationType);

            //Func<CookieResponseSignInContext, Task> initAuthAsync = async ctx =>
            //{
            //    ctx.Identity = await StartupAuth.InitAuthAsync(ctx);
            //};

            var authProvider = new CookieAuthenticationProvider
            {
                OnResponseSignIn = ctx =>
                {
                    //initAuthAsync(ctx).Wait(8000);
                    StartupAuth.InitAuth(ctx);
                }
            };

            var cookieOptions = new CookieAuthenticationOptions
            {
                Provider = authProvider
            };

            app.UseCookieAuthentication(cookieOptions);

            // Required for AAD Multitenant (Pre-auth access to home tenant during sign-up)
            OpenIdConnectAuthenticationOptions multiOptions = new OpenIdConnectAuthenticationOptions
            {
                Authority = aadInstanceMulti,
                ClientId = clientId_preAuth,
                Notifications = new OpenIdConnectAuthenticationNotifications
                {
                    AuthenticationFailed = AuthenticationFailed,
                    RedirectToIdentityProvider = (context) =>
                    {
                        string appBaseUrl = context.Request.Scheme + "://" + context.Request.Host + context.Request.PathBase;
                        context.ProtocolMessage.RedirectUri = appBaseUrl + "/";
                        context.ProtocolMessage.PostLogoutRedirectUri = appBaseUrl;
                        return Task.FromResult(0);
                    },
                },
                TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                },
                AuthenticationType = AuthTypes.B2EMulti,
            };
            app.UseOpenIdConnectAuthentication(multiOptions);

            // Required for AAD B2B
            OpenIdConnectAuthenticationOptions b2bOptions = new OpenIdConnectAuthenticationOptions
            {
                Authority = string.Format(aadInstanceLocal, tenant),
                ClientId = clientId_admin,
                Notifications = new OpenIdConnectAuthenticationNotifications
                {
                    AuthenticationFailed = AuthenticationFailed,
                    RedirectToIdentityProvider = (context) =>
                    {
                        string appBaseUrl = context.Request.Scheme + "://" + context.Request.Host + context.Request.PathBase;
                        context.ProtocolMessage.RedirectUri = appBaseUrl + "/";
                        context.ProtocolMessage.PostLogoutRedirectUri = appBaseUrl;
                        return Task.FromResult(0);
                    },
                },
                TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                },
                AuthenticationType = AuthTypes.Local,
            };
            app.UseOpenIdConnectAuthentication(b2bOptions);
        }

        // Used for avoiding yellow-screen-of-death
        private Task AuthenticationFailed(AuthenticationFailedNotification<OpenIdConnectMessage, OpenIdConnectAuthenticationOptions> notification)
        {
            notification.HandleResponse();
            if (notification.Exception.Message == "access_denied")
            {
                notification.Response.Redirect("/");
            }
            else
            {
                notification.Response.Redirect("/Home/Error?message=" + notification.Exception.Message);
            }

            return Task.FromResult(0);
        }
    }
}



/*
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
                    }
                });
        }
    }
}
*/