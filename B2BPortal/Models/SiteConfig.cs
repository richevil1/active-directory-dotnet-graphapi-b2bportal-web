using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Web.Mvc;
using B2BPortal.Data;
using B2BPortal.Infrastructure;
using B2BPortal.Interfaces;
using AzureB2BInvite;
using Westwind.Web.Utilities;

namespace B2BPortal.Models
{
    public class SiteConfig : DocModelBase, IDocModelBase
    {
        /// <summary>
        /// Site name displayed on home page (above description)
        /// </summary>
        [DisplayName("Site Name")]
        [JsonProperty(PropertyName = "siteName")]
        public string SiteName { get; set; }

        /// <summary>
        /// Name of inviting organization
        /// </summary>
        [DisplayName("Inviting Organization")]
        [JsonProperty(PropertyName = "invitingOrg")]
        public string InvitingOrg { get; set; }

        /// <summary>
        /// Name of inviting organization
        /// </summary>
        [DisplayName("Email displayed in invitation email for replies")]
        [JsonProperty(PropertyName = "inviterResponseEmailAddr")]
        public string InviterResponseEmailAddr { get; set; }

        /// <summary>
        /// URL the guest is returned to after an invitation is redeemed.
        /// In the UX, a selector is presented offering to return to the Profile editor, the org "MyApps" page, or a custom URL.
        /// (custom domain pre-auth records also define this URL - if they exist, they will override this setting)
        /// </summary>
        [DisplayName("Return URL after an invite is redeemed")]
        [JsonProperty(PropertyName = "inviteRedirectUrl")]
        public string InviteRedirectUrl { get; set; }

        /// <summary>
        /// Welcome message displayed on the home page - HTML allowed
        /// </summary>
        [DisplayName("Welcome Message")]
        [JsonProperty(PropertyName = "welcomeMessage")]
        public string WelcomeMessage { get; set; }

        /// <summary>
        /// Are guests only allowed to request access when their login credential matches a domain preauth, and 
        /// pre-authentication to their home domain is completed?
        /// </summary>
        [DisplayName("Require Preauth")]
        [JsonProperty(PropertyName = "requirePreauth")]
        public bool RequirePreauth { get; set; }

        /// <summary>
        /// TODO: Should users be informed via email when their request was denied? (Requires SMTP)
        /// </summary>
        [DisplayName("Send Denial Notification")]
        [JsonProperty(PropertyName = "sendDenial")]
        public bool SendDenial { get; set; }

        /// <summary>
        /// Is clicking the TOS checkbox required before allowing the guest request to be submitted?
        /// </summary>
        [DisplayName("Require TOS Agreement")]
        [JsonProperty(PropertyName = "requireTosAgreement")]
        public bool RequireTOSAgreement { get; set; }

        /// <summary>
        /// Full-text of Terms of Service - HTML permissible
        /// </summary>
        [DisplayName("TOS Document")]
        [AllowHtml]
        [JsonProperty(PropertyName = "tosDocument")]
        public string TOSDocument { get; set; }

        /// <summary>
        /// Date this version of settings was committed
        /// </summary>
        [ScaffoldColumn(false)]
        [DisplayName("Config Date")]
        [JsonProperty(PropertyName = "configDate")]
        public DateTime ConfigDate { get; set; }

        [ScaffoldColumn(false)]
        [JsonProperty(PropertyName = "configVersion")]
        [DisplayName("Config Version")]
        public int ConfigVersion { get; set; }

        /// <summary>
        /// UPN of author of this config version
        /// </summary>
        [JsonProperty(PropertyName = "configAuthor")]
        [DisplayName("Config Author")]
        public string ConfigAuthor { get; set; }

        public static async Task<IEnumerable<SiteConfig>> GetAllConfigs()
        {
            return (await DocDBRepo.DB<SiteConfig>.GetItemsAsync()).OrderByDescending(c => c.ConfigDate);
        }

        public static async Task<SiteConfig> GetCurrConfig()
        {
            var items = await DocDBRepo.DB<SiteConfig>.GetItemsAsync();
            return items.OrderBy(c => c.ConfigDate).LastOrDefault();
        }

        public static async Task<SiteConfig> GetConfig(string id)
        {
            return (await DocDBRepo.DB<SiteConfig>.GetItemAsync(id));
        }

        public static async Task<SiteConfig> SetNewConfig(SiteConfig config)
        {
            config.ConfigDate = DateTime.UtcNow;
            config.ConfigVersion++;

            //TOSDocument is decorated with [AllowHtml], so clearing out dangerous tags
            config.TOSDocument = HtmlSanitizer.SanitizeHtml(config.TOSDocument);

            config = (await DocDBRepo.DB<SiteConfig>.CreateItemAsync(config));
            Settings.SiteConfigReady = true;
            Settings.CurrSiteConfig = config;

            //refresh settings elsewhere in the app
            MailSender.MailFrom = Settings.CurrSiteConfig.InviterResponseEmailAddr;
            AdalUtil.Settings.InviterResponseEmailAddr = Settings.CurrSiteConfig.InviterResponseEmailAddr;
            AdalUtil.Settings.DefaultRedirectUrl = Settings.CurrSiteConfig.InviteRedirectUrl;

            return config;
        }
    }
}