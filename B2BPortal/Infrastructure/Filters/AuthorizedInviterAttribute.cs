using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using B2BPortal.Infrastructure;
using AzureB2BInvite;

namespace B2BPortal.Infrastructure.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
    public class AuthorizedInviterAttribute : AuthorizeAttribute
    {
        /// <summary>
        /// Check authorization
        /// </summary>
        /// <param name="filterContext"></param>
        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            var currentUser = filterContext.HttpContext.User.Identity;
            if (!currentUser.IsInAnyRole(AdalUtil.Settings.InviterRoleNames))
            {
                filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary
                {
                    { "controller", "home" }, { "action", "error" }
                });
            }
        }
    }
}