using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace B2BPortal.Models
{
    public class DocModelBase
    {
        [JsonProperty(PropertyName = "docType")]
        public DocTypes DocType { get; set; }

    }
}