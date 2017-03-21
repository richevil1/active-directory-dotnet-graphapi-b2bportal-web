using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace B2BPortal.Models
{
    public class GraphEmail
    {
        public Message message;
        public List<Recipient> recipients;
    }
}