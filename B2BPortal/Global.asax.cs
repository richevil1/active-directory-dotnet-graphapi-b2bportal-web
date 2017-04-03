using B2BPortal.Data;
using B2BPortal.Infrastructure;
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
using AzureB2BInvite;

namespace B2BPortal
{
    public class MvcApplication : HttpApplication
    {
        protected void Application_Start()
        {
            try
            {
                //DocDB config

                DocDBRepo.Settings.DocDBUri = ConfigurationManager.AppSettings["DocDBUri"];
                DocDBRepo.Settings.DocDBAuthKey = ConfigurationManager.AppSettings["DocDBAuthKey"];
                DocDBRepo.Settings.DocDBName = ConfigurationManager.AppSettings["DocDBName"];
                DocDBRepo.Settings.DocDBCollection = ConfigurationManager.AppSettings["DocDBCollection"];

                var client = DocDBRepo.Initialize().Result;
                var s = client.AuthKey;

                ControllerBuilder.Current.DefaultNamespaces.Add("B2BPortal.Controllers");
                AreaRegistration.RegisterAllAreas();
                GlobalConfiguration.Configure(WebApiConfig.Register);
                FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
                RouteConfig.RegisterRoutes(RouteTable.Routes);
                BundleConfig.RegisterBundles(BundleTable.Bundles);
                AntiForgeryConfig.UniqueClaimTypeIdentifier = "http://schemas.microsoft.com/identity/claims/objectidentifier";

                //Settings
                var isConfig = Settings.LoadCurrSiteConfig();
                if (isConfig)
                {
                    //if new site, no config but invites are disabled until config is complete
                    AdalUtil.Settings.SiteRedemptionSettings = Settings.CurrSiteConfig.SiteRedemptionSettings;
                    MailSender.MailFrom = Settings.CurrSiteConfig.SiteRedemptionSettings.InviterResponseEmailAddr;
                }

                AdalUtil.Settings.AADInstanceLocal = ConfigurationManager.AppSettings["ida:AADInstanceLocal"];
                AdalUtil.Settings.AADInstanceMulti = ConfigurationManager.AppSettings["ida:AADInstanceMulti"];
                AdalUtil.Settings.TenantID = ConfigurationManager.AppSettings["ida:TenantId"];

                AdalUtil.Settings.AppClientId_Admin = ConfigurationManager.AppSettings["ida:ClientId_Admin"];
                AdalUtil.Settings.AppClientSecret_Admin = ConfigurationManager.AppSettings["ida:ClientSecret_Admin"];
                AdalUtil.Settings.AppClientId_Preauth = ConfigurationManager.AppSettings["ida:ClientId_PreAuth"];
                AdalUtil.Settings.AppClientSecret_Preauth = ConfigurationManager.AppSettings["ida:ClientSecret_PreAuth"];

                AdalUtil.Settings.GraphApiVersion = ConfigurationManager.AppSettings["GraphApiVersion"];

                AdalUtil.Settings.InvitationEmailSubject = ConfigurationManager.AppSettings["InvitationEmailSubject"];
                AdalUtil.Settings.DefaultBodyTemplateName = ConfigurationManager.AppSettings["DefaultBodyTemplateName"];
                AdalUtil.Settings.InviterRoleNames = (ConfigurationManager.AppSettings["InviterRoleNames"] as string).Split(',');
                AdalUtil.Settings.AssignedInviterRole = ConfigurationManager.AppSettings["AssignedInviterRole"];

                //SMTP config
                MailSender.MailEnabled = (ConfigurationManager.AppSettings["MailEnabled"] == "1");
                MailSender.LogoPath = Server.MapPath(ConfigurationManager.AppSettings["MailLogoPath"]);
                MailSender.MailTemplate = Settings.GetMailTemplate(ConfigurationManager.AppSettings["MailTemplateName"]);
                MailSender.MailServer = ConfigurationManager.AppSettings["MailServer"];
                MailSender.MailServerPort = Convert.ToInt32(ConfigurationManager.AppSettings["SMTPPort"]);
                MailSender.SMTPLogin = ConfigurationManager.AppSettings["SMTPLogin"];
                MailSender.SMTPPassword = ConfigurationManager.AppSettings["SMTPPassword"];

                Settings.UseSMTP = (!string.IsNullOrEmpty(MailSender.MailServer));
                AdalUtil.Settings.UseSMTP = Settings.UseSMTP;

                /*
                 * TODO: Prefetching the app token here because initializing this library during admin
                 * authentication is timing out/failing.
                 * don't know if this is due to the api call or spinning up this code
                 * see AdalUtil.CallGraph...
                */
                AdalUtil.AuthenticateApp();
            }
            catch (Exception ex)
            {
                Logging.WriteToAppLog("Error during site initialization", System.Diagnostics.EventLogEntryType.Error, ex);
                throw;
            }
        }
    }
}
