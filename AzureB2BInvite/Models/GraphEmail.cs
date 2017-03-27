using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AzureB2BInvite.Models
{
    public class GraphEmail
    {
        public Message message;
        public List<Recipient> recipients;
    }
}