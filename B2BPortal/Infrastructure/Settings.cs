using B2BPortal.B2B;
using B2BPortal.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Web;

namespace B2BPortal.Infrastructure
{
    public static class Settings
    {
        public static string AppRootPath = HttpContext.Current.Server.MapPath("//");
        public static string InviterUPN { get; set; }
        public static string AADInstanceLocal { get; set; }
        public static string AADInstanceMulti { get; set; }
        public static string GraphApiVersion { get; set; }
        public static string TenantID { get; set; }

        public static string AppClientId_Admin { get; set; }
        public static string AppClientSecret_Admin { get; set; }

        public static string StorageConnectionString { get; set; }

        public static string RequestsTableName = "TableRequests";

        public static string GraphResource = "https://graph.microsoft.com";
        public static string InviteRedirectUrl { get; set; }

        public static string InvitationEmailSubject { get; set; }
        public static string[] InviterRoleNames { get; set; }
        public static string AssignedInviterRole { get; set; }

        public static IEnumerable<GraphRoleUser> AssignedInviteRoleUsers { get; set; }

        /// <summary>
        /// An array of paths that are allowed access by a multi-tenant authenticated visitor.
        /// These visitors are here for pre-authentication to the request page and to the "thank you" page
        /// after a request.
        /// </summary>
        public static string[] VisitorAllowedPaths = { @"/", @"/profile/signup", @"/account/signin" };

        public static string InvitingOrganization { get; set; }
        public static string GetMailTemplate(string templateName)
        {
            var mailPath = Path.Combine(AppRootPath, @"Templates\" + templateName);
            return File.ReadAllText(mailPath);
        }
    }
}