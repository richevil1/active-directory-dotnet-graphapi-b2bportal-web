using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using B2BPortal.Infrastructure;
using B2BPortal.Models;
using B2BPortal.Infrastructure.Filters;
using B2BPortal.Data;

namespace B2BPortal.Areas.Admin.Controllers
{
    [AuthorizedInviter]
    public class InviteTemplateController : Controller
    {
        public async Task<ActionResult> Index()
        {
            var templates = await TemplateUtilities.GetTemplates();
            if (templates.Count() == 0)
            {
                templates = await TemplateUtilities.InitializeDefaultTemplate(User.Identity.GetEmail());
            }
            return View(templates);
        }

        public async Task<ActionResult> Edit(string id)
        {
            InviteTemplate template;
            if (id == null)
            {
                ViewBag.Operation = "New";
                template = new InviteTemplate();
            }
            else
            {
                ViewBag.Operation = "Edit";
                template = await TemplateUtilities.GetTemplate(id);
            }
            return View(template);
        }

        [HttpPost]
        public async Task<bool> Delete(InviteTemplate template)
        {
            var res = await TemplateUtilities.DeleteTemplate(template);
            return true;
        }

        [HttpPost]
        public async Task<ActionResult> Save(InviteTemplate template)
        {
            if (ModelState.IsValid)
            {
                //check for subject template if smtp
                if (Settings.UseSMTP)
                {
                    if (string.IsNullOrEmpty(template.SubjectTemplate))
                    {
                        ModelState.AddModelError("SubjectTemplate", "With SMTP enabled, the subject template is required.");
                        return View("Edit", template);
                    }
                }
                try
                {
                    template.TemplateAuthor = User.Identity.GetEmail();
                    if (template.Id == null)
                    {
                        template = await TemplateUtilities.AddTemplate(template);
                    }
                    else
                    {
                        template = await TemplateUtilities.UpdateTemplate(template);
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
