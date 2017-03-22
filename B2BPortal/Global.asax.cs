using B2BPortal.B2B;
using B2BPortal.Data;
using B2BPortal.Infrastructure;
using B2BPortal.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace B2BPortal
{
    public class MvcApplication : HttpApplication
    {
        protected void Application_Start()
        {
            ControllerBuilder.Current.DefaultNamespaces.Add("B2BPortal.Controllers");
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            GlobalConfiguration.Configure(WebApiConfig.Register);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            AntiForgeryConfig.UniqueClaimTypeIdentifier = "http://schemas.microsoft.com/identity/claims/objectidentifier";

            //Settings
            Settings.InviterUPN = ConfigurationManager.AppSettings["ida:InviterUpn"];
            Settings.AADInstanceLocal = ConfigurationManager.AppSettings["ida:AADInstanceLocal"];
            Settings.AADInstanceMulti = ConfigurationManager.AppSettings["ida:AADInstanceMulti"];
            Settings.TenantID = ConfigurationManager.AppSettings["ida:TenantId"];

            Settings.AppClientId_Admin = ConfigurationManager.AppSettings["ida:ClientId_Admin"];
            Settings.AppClientSecret_Admin = ConfigurationManager.AppSettings["ida:ClientSecret_Admin"];
            
            Settings.GraphApiVersion = ConfigurationManager.AppSettings["GraphApiVersion"];

            Settings.StorageConnectionString = ConfigurationManager.AppSettings["StorageConnectionString"];
            Settings.InviteRedirectUrl = ConfigurationManager.AppSettings["InviteRedirectUrl"];
            Settings.InvitationEmailSubject = ConfigurationManager.AppSettings["InvitationEmailSubject"];
            Settings.InvitingOrganization = ConfigurationManager.AppSettings["InvitingOrganization"];
            Settings.InviterRoleNames = (ConfigurationManager.AppSettings["InviterRoleNames"] as string).Split(',');
            Settings.AssignedInviterRole = ConfigurationManager.AppSettings["AssignedInviterRole"];

            //SendGrid config
            MailSender.MailEnabled = (ConfigurationManager.AppSettings["MailEnabled"] == "1");
            MailSender.LogoPath = Server.MapPath(ConfigurationManager.AppSettings["MailLogoPath"]);
            MailSender.MailTemplate = Settings.GetMailTemplate(ConfigurationManager.AppSettings["MailTemplateName"]);
            MailSender.MailFrom = Settings.InviterUPN;
            MailSender.MailServer = ConfigurationManager.AppSettings["MailServer"];
            MailSender.MailServerPort = Convert.ToInt32(ConfigurationManager.AppSettings["SMTPPort"]);
            MailSender.SMTPLogin = ConfigurationManager.AppSettings["SMTPLogin"];
            MailSender.SMTPPassword = ConfigurationManager.AppSettings["SMTPPassword"];

            //DocDB config
            DocDBSettings.DocDBUri = ConfigurationManager.AppSettings["DocDBUri"];
            DocDBSettings.DocDBAuthKey = ConfigurationManager.AppSettings["DocDBAuthKey"];
            DocDBSettings.DocDBName = ConfigurationManager.AppSettings["DocDBName"];
            DocDBSettings.DocDBCollection = ConfigurationManager.AppSettings["DocDBCollection"];
            DocDBRepo<GuestRequest>.Initialize();
            DocDBRepo<PreAuthDomain>.Initialize();

            /*
             * TODO: Prefetching the app token here because initializing this library during admin
             * authentication is timing out/failing.
             * don't know if this is due to the api call or spinning up this code
             * see AdalUtil.CallGraph...
            */
            AdalUtil.Authenticate();
        }
    }
}
