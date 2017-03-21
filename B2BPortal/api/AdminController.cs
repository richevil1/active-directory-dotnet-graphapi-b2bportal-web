using B2BPortal.Infrastructure;
using B2BPortal.Infrastructure.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace B2BPortal.api
{
    [AuthorizedInviter]
    public class AdminController : ApiController
    {
    }
}
