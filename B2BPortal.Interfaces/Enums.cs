using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace B2BPortal.Interfaces
{
    /// <summary>
    /// Filter for DocumentDB doc types - any records stored in DocumentDB need their type names added here
    /// </summary>
    public enum DocTypes
    {
        PreAuthDomain,
        GuestRequest,
        SiteConfig,
        InviteTemplate
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