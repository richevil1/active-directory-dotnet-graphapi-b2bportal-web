using System;
using System.Collections.Generic;
using System.Linq;
using static AzureB2BInvite.AdalUtil;
using AzureB2BInvite.Models;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System.Security.Claims;
using System.Threading;
using System.Globalization;
using System.Net.Http;
using System.Web;
using System.Net.Http.Headers;

namespace AzureB2BInvite
{
    public class ProfileManager
    {
        public static AADUserProfile GetUserProfile(string upn)
        {
            var userUri = string.Format("{0}/{1}/users/{2}", Settings.GraphResource, Settings.GraphApiVersion, upn);
            var serverResponse = CallGraph(userUri);

            AADUserProfile res = JsonConvert.DeserializeObject<AADUserProfile>(serverResponse.ResponseContent);
            return res;
        }

        public static void UpdateProfile(dynamic user, string upn)
        {
            var userUri = string.Format("{0}/{1}/users/{2}", Settings.GraphResource, Settings.GraphApiVersion, upn);
            var serverResponse = CallGraph(userUri, user, true);
        }

        public static async Task<string> GetRedirUrl(string upn)
        {
            var domainName = upn.Split('@')[1];
            var domProfile = (await PreAuthDomain.GetDomains(d => d.DomainName == domainName)).SingleOrDefault();
            var res = (domProfile == null) ? Settings.SiteRedemptionSettings.InviteRedirectUrl :
                (!string.IsNullOrEmpty(domProfile.DomainRedemptionSettings.InviteRedirectUrl))
                    ? domProfile.DomainRedemptionSettings.InviteRedirectUrl
                        : Settings.SiteRedemptionSettings.InviteRedirectUrl;

            return res;
        }
    }
}