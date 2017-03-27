using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AzureB2BInvite.Models
{
    public class GraphMemberRole
    {
        [JsonProperty(PropertyName = "@odata.type")]
        public string Type { get; set; }

        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }

        [JsonProperty(PropertyName = "displayName")]
        public string DisplayName { get; set; }

        [JsonProperty(PropertyName = "roleTemplateId")]
        public string RoleTemplateId { get; set; }
    }
}