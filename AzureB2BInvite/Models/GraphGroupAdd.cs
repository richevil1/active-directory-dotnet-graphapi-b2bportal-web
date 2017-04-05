using Newtonsoft.Json;
using System;

namespace AzureB2BInvite.Models
{
    public class GraphGroupAdd
    {
        private string _id;

        [JsonProperty(PropertyName = "@odata.id")]
        public string Id {
            get { return _id; }
            set {
                _id = string.Format("https://graph.microsoft.com/v1.0/directoryObjects/{0}", value);
            }
        }

        public GraphGroupAdd(string userId)
        {
            Id = userId;
            //{\"@odata.id\": \"https://graph.microsoft.com/v1.0/directoryObjects/{0}\"}
        }
    }
}