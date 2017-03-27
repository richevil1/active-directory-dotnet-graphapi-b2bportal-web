using AzureB2BInvite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Filters;

namespace B2BPortal.Infrastructure.Filters
{
    public class CheckVisitorAttribute : ActionFilterAttribute, IAuthenticationFilter
    {
        public void OnAuthentication(AuthenticationContext filterContext)
        {
            var currTenant = filterContext.Principal.Identity.GetClaim(CustomClaimTypes.TenantId);

            //bail out if unauthenticated or if we're already logged into the home tenant
            if (!filterContext.Principal.Identity.IsAuthenticated || currTenant == AdalUtil.Settings.TenantID )
            {
                return;
            }

            //check maintenance mode for all other authenticated users (currently, show maint page or allow logoff)
            //if (Settings.TenantID != currTenant && !Settings.VisitorAllowedPaths.Any(s => filterContext.HttpContext.Request.RawUrl.ToLower() == s))
            //{
            //    filterContext.Result = new ViewResult
            //    {
            //        ViewName = "~/Views/Shared/PreAuthError.cshtml"
            //    };
            //}
        }

        public void OnAuthenticationChallenge(AuthenticationChallengeContext filterContext)
        {

        }
    }
}

