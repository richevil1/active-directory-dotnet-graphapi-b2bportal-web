using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace B2BPortal.Models
{
    public class GraphInvitation
    {
        public string InvitedUserDisplayName { get; set; }
        public string InvitedUserEmailAddress { get; set; }
        public bool SendInvitationMessage { get; set; }
        public string InviteRedirectUrl { get; set; }
    }
}