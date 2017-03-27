using AzureB2BInvite.Models;
using B2BPortal.Data;
using B2BPortal.Infrastructure.Filters;
using B2BPortal.Infrastructure;
using B2BPortal.Models;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace B2BPortal.Areas.Admin.Controllers
{
    [AuthorizedInviter]
    public class PreApprovalController : AsyncController
    {
        private List<SelectListItem> _templates;

        public PreApprovalController()
        {
            var task = Task.Run(async () => {
                var templates = (await InviteTemplate.GetTemplates());
                _templates = new List<SelectListItem>();
                _templates.Add(new SelectListItem { Selected = true, Text = "Select optional email template", Value = "" });
                _templates.AddRange(templates.Select(t => new SelectListItem { Selected = false, Text = t.TemplateName, Value = t.Id }));
            });
            task.Wait();
        }
        public async Task<ActionResult> Index()
        {
            IEnumerable<PreAuthDomain> domains = await PreAuthDomain.GetDomains();
            return View(domains);
        }

        [HttpPost]
        public async Task<ActionResult> Create(PreAuthDomain domain)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    domain.AuthUser = User.Identity.GetEmail();
                    var doc = await PreAuthDomain.AddDomain(domain);
                    return RedirectToAction("Index");
                }
                catch
                {
                    return View("Edit", domain);
                }
            }
            return View("Edit", domain);
        }

        public async Task<ActionResult> Edit(string id)
        {
            PreAuthDomain domain;
            if (id == null)
            {
                ViewBag.Operation = "Create";
                domain = new PreAuthDomain();
            }
            else
            {
                ViewBag.Operation = "Edit";
                domain = await PreAuthDomain.GetDomain(id);
            }

            var templates = _templates.Select(t => new SelectListItem {
                Selected = (t.Value==domain.InvitationTemplate),
                Text = t.Text,
                Value = t.Value
            });

            ViewBag.TemplateList = templates;
            return View(domain);
        }

        public async Task<ActionResult> Details(string id)
        {
            PreAuthDomain domain = await PreAuthDomain.GetDomain(id);
            if (domain.InvitationTemplate.Length > 0)
            {
                domain.InvitationTemplate = string.Format("{0} ({1})", domain.InvitationTemplate, _templates.Where(t => t.Value == domain.InvitationTemplate).Single().Text);
            }
            ViewBag.Operation = "Details";
            return View(domain);
        }

        [HttpPost]
        public async Task<ActionResult> Edit(PreAuthDomain preAuthDomain)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (!string.IsNullOrEmpty(preAuthDomain.GroupsList) && preAuthDomain.GroupsList.Length > 0)
                    {
                        preAuthDomain.Groups = new List<string>(preAuthDomain.GroupsList.Split(','));
                    }
                    preAuthDomain.AuthUser = User.Identity.GetEmail();
                    preAuthDomain = await PreAuthDomain.UpdateDomain(preAuthDomain);

                    return RedirectToAction("Index");
                }
                catch
                {
                    ViewBag.Operation = "Edit";
                    return View(preAuthDomain);
                }
            }
            return View(preAuthDomain);
        }

        [HttpPost]
        public async Task<ActionResult> Delete(PreAuthDomain domain)
        {
            try
            {
                await PreAuthDomain.DeleteDomain(domain);
                return RedirectToAction("Index");
            }
            catch
            {
                ViewBag.Operation = "Delete";
                return RedirectToAction("Index");
            }
        }
    }
}