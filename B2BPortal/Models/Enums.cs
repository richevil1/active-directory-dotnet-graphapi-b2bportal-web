using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace B2BPortal.Models
{
    /// <summary>
    /// Filter for DocumentDB doc types
    /// </summary>
    public enum DocTypes
    {
        PreAuthDomains,
        GuestRequest
    }

    /// <summary>
    /// List of possible guest request approval choices
    /// </summary>
    public enum Disposition
    {
        Approved,
        AutoApproved,
        Denied,
        Pending
    }

}