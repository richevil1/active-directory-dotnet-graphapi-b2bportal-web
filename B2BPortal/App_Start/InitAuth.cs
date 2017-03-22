using System;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Web;
using Microsoft.Owin.Security.Cookies;
using B2BPortal.Infrastructure;
using B2BPortal.B2B;
using System.Threading.Tasks;

namespace B2BPortal
{
    public static class StartupAuth
    {
        public static ClaimsIdentity InitAuth(CookieResponseSignInContext ctx)
        {
            var ident = ctx.Identity;
            var hctx =
                (HttpContextWrapper)
                    ctx.Request.Environment.Single(e => e.Key == "System.Web.HttpContextBase").Value;

            var initResults = InitAuth(ident, hctx);
            return initResults;
        }

        private static ClaimsIdentity InitAuth(ClaimsIdentity ident, HttpContextBase hctx)
        {
            try
            {
                //if this is a multi-tenant visitor, we don't need to do anything here
                if (ident.GetClaim(CustomClaimTypes.TenantId) != Settings.TenantID)
                {
                    ident.AddClaim(new Claim(CustomClaimTypes.AuthType, AuthTypes.B2EMulti));
                    return ident;
                }

                ident = TransformClaims(ident);

                return ident;
            }
            catch (Exception ex)
            {
                Debug.Assert(hctx.Session != null, "hctx.Session != null");
                hctx.Session["AuthError"] = "There was an error authenticating. Please contact the system administrator.";
                Logging.WriteToAppLog("Error during InitAuth.", EventLogEntryType.Error, ex);
                throw;
            }
        }
        private static ClaimsIdentity TransformClaims(ClaimsIdentity ident)
        {
            var issuer = ident.Claims.First().Issuer;

            ident.AddClaim(new Claim(CustomClaimTypes.AuthType, AuthTypes.Local));

            var roles = InviteManager.GetDirectoryRoles(ident.GetClaim(ClaimTypes.Upn));

            foreach(var role in roles)
            {
                ident.AddClaim(new Claim(ClaimTypes.Role, role.DisplayName));
            }

            var fullName = ident.Claims.FirstOrDefault(c => c.Type == "name").Value;
            ident.AddClaim(new Claim(CustomClaimTypes.FullName, fullName));

            return ident;
        }
    }
}