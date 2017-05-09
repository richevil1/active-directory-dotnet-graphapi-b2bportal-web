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
using AzureB2BInvite.Models;

namespace B2BPortal.Models
{
    public class SiteConfig : DocModelBase, IDocModelBase
    {
        public SiteConfig()
        {
            SiteRedemptionSettings = new RedemptionSettings();
            SiteRedemptionSettings.InviteRedirectUrl = string.Format("https://myapps.microsoft.com/{0}", AdalUtil.Settings.Tenant);
            InviteTemplateContent = new InviteTemplate();
        }

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
        /// Welcome message displayed on the home page - HTML allowed
        /// </summary>
        [DisplayName("Welcome Message")]
        [JsonProperty(PropertyName = "welcomeMessage")]
        public string WelcomeMessage { get; set; }

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
        /// Default site-wide redemption settings - will be in effect for non-preauthed domain invitations
        /// </summary>
        [DisplayName("Site Redemption Settings")]
        [JsonProperty(PropertyName = "siteRedemptionSettings")]
        public RedemptionSettings SiteRedemptionSettings { get; set; }

        /// <summary>
        /// RecordID for the email body template to use for default pre-auth invitations not handled by a pre-auth domain record
        /// </summary>
        [ScaffoldColumn(false)]
        [DisplayName("Invitation Template")]
        [JsonProperty(PropertyName = "inviteTemplateId")]
        public string InviteTemplateId { get; set; }

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

        [JsonIgnore]
        [ScaffoldColumn(false)]
        public InviteTemplate InviteTemplateContent { get; set; }

        public static async Task<IEnumerable<SiteConfig>> GetAllConfigs()
        {
            return (await DocDBRepo.DB<SiteConfig>.GetItemsAsync()).OrderByDescending(c => c.ConfigDate);
        }

        public static async Task<SiteConfig> GetCurrConfig()
        {
            var items = await DocDBRepo.DB<SiteConfig>.GetItemsAsync();
            var res = items.OrderBy(c => c.ConfigDate).LastOrDefault();
            if (res!=null && res.InviteTemplateId != null)
            {
                res.InviteTemplateContent = (await DocDBRepo.DB<InviteTemplate>.GetItemAsync(res.InviteTemplateId));
            }
            return res;
        }

        public static async Task<SiteConfig> GetConfig(string id)
        {
            var res = (await DocDBRepo.DB<SiteConfig>.GetItemAsync(id));
            if (res.InviteTemplateId != null)
            {
                res.InviteTemplateContent = (await DocDBRepo.DB<InviteTemplate>.GetItemAsync(res.InviteTemplateId));
            }
            return res;
        }

        public static async Task<SiteConfig> SetNewConfig(SiteConfig config)
        {
            config.ConfigDate = DateTime.UtcNow;
            config.ConfigVersion++;

            //TOSDocument is decorated with [AllowHtml], so clearing out dangerous tags
            if (!string.IsNullOrEmpty(config.TOSDocument))
            {
                config.TOSDocument = HtmlSanitizer.SanitizeHtml(config.TOSDocument);
            }

            config = (await DocDBRepo.DB<SiteConfig>.CreateItemAsync(config));
            Settings.SiteConfigReady = true;
            Settings.CurrSiteConfig = config;

            //refresh invitation settings
            AdalUtil.Settings.SiteRedemptionSettings = Settings.CurrSiteConfig.SiteRedemptionSettings;

            return config;
        }
    }
}