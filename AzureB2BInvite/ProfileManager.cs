using System;
using System.Collections.Generic;
using System.Linq;
using static AzureB2BInvite.AdalUtil;
using AzureB2BInvite.Models;
using Newtonsoft.Json;

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
    }
}
