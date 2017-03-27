using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using B2BPortal.Infrastructure;
using B2BPortal.Models;
using B2BPortal.Infrastructure.Filters;

namespace B2BPortal.Areas.Admin.Controllers
{
    [AuthorizedInviter]
    public class InviteTemplateController : Controller
    {
        public async Task<ActionResult> Index()
        {
            var templates = await InviteTemplate.GetTemplates();
            if (templates.Count() == 0)
            {
                templates = await InviteTemplate.InitializeDefaultTemplate(User.Identity.GetEmail());
            }
            return View(templates);
        }

        public async Task<ActionResult> Details(string id)
        {
            var template = await InviteTemplate.GetTemplate(id);
            return View(template);
        }

        public async Task<ActionResult> Edit(string id)
        {
            InviteTemplate template;
            if (id == null)
            {
                template = new InviteTemplate();
            } else
            {
                template = await InviteTemplate.GetTemplate(id);

            }
            return View(template);
        }

        [HttpPost]
        public async Task<ActionResult> Save(InviteTemplate template)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    template.TemplateAuthor = User.Identity.GetEmail();
                    if (template.Id == null)
                    {
                        template = await InviteTemplate.AddTemplate(template);
                    }
                    else
                    {
                        template = await InviteTemplate.UpdateTemplate(template);
                    }
                    return RedirectToAction("Index");
                }
                catch
                {
                    return View("Edit", template);
                }
            }
            return View("Edit", template);
        }
    }
}
