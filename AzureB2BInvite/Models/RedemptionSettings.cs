using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace AzureB2BInvite.Models
{
    public class RedemptionSettings
    {
        /// <summary>
        /// Are guests only allowed to request access when their login credential matches a domain preauth, and 
        /// pre-authentication to their home domain is completed?
        /// </summary>
        [DisplayName("Require Sign-In")]
        [JsonProperty(PropertyName = "requirePreauth")]
        public bool RequirePreauth { get; set; }

        /// <summary>
        /// URL the guest is returned to after an invitation is redeemed.
        /// In the UX, a selector is presented offering to return to the Profile editor, the org "MyApps" page, or a custom URL.
        /// (custom domain pre-auth records also define this URL - if they exist, they will override this setting)
        /// </summary>
        [DisplayName("Return URL After Profile Edit")]
        [JsonProperty(PropertyName = "inviteRedirectUrl")]
        public string InviteRedirectUrl { get; set; }

        /// <summary>
        /// Default email address for contact from an invitee or in the custom email content
        /// </summary>
        [DisplayName("Inviter Email Address")]
        [JsonProperty(PropertyName = "inviterResponseEmailAddr")]
        public string InviterResponseEmailAddr { get; set; }
    }
}