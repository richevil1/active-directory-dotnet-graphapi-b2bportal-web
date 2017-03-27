using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AzureB2BInvite.Models
{
    public class GraphRoleUser
    {
        [JsonProperty(PropertyName = "accountEnabled")]
        public bool AccountEnabled { get; set; }

        [JsonProperty(PropertyName = "displayName")]
        public string DisplayName { get; set; }

        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "userPrincipalName")]
        public string UserPrincipalName { get; set; }

        [JsonProperty(PropertyName = "userType")]
        public string UserType { get; set; }

        public GraphRoleUser()
        {

        }
        public GraphRoleUser(dynamic user)
        {
            UserPrincipalName = user.userPrincipalName;
            UserType = user.userType;
            Id = user.id;
            DisplayName = user.displayName;
            AccountEnabled = user.accountEnabled;
        }
    }
}