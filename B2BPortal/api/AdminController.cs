using AzureB2BInvite;
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
        [HttpGet]
        public TemplateSettings GetStaticDefaultTemplate()
        {
            return new TemplateSettings
            {
                SubjectTemplate = AdalUtil.Settings.InvitationEmailSubject,
                BodyTemplate = Settings.GetMailTemplate(AdalUtil.Settings.DefaultBodyTemplateName)
            };
        }
    }
    public class TemplateSettings
    {
        public string SubjectTemplate { get; set; }
        public string BodyTemplate { get; set; }
    }
}
