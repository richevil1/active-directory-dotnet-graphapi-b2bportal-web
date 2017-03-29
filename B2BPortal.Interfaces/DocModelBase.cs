using Newtonsoft.Json;

namespace B2BPortal.Interfaces
{
    public class DocModelBase
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "docType")]
        public DocTypes DocType { get; set; }
    }
}