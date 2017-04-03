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
using AzureB2BInvite;

namespace B2BPortal.Areas.Admin.Controllers
{
    [AuthorizedInviter]
    public class PreApprovalController : AsyncController
    {
        private List<SelectListItem> _templates;
        private List<SelectListItem> _groups;

        public PreApprovalController()
        {
            var task = Task.Run(async () => {
                var templates = (await TemplateUtilities.GetTemplates());
                _templates = new List<SelectListItem>();
                _templates.Add(new SelectListItem { Selected = true, Text = "Select optional email template", Value = "" });
                _templates.AddRange(templates.Select(t => new SelectListItem { Selected = false, Text = t.TemplateName, Value = t.Id }));

                var groups = (await new GraphUtil().GetGroups());
                _groups = new List<SelectListItem>();
                _groups.AddRange(groups.Select(g => new SelectListItem { Selected = false, Text = g.DisplayName, Value = g.Id }));
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
            ViewBag.Templates = _templates;
            ViewBag.Groups = _groups;

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
            ViewBag.Templates = _templates;
            ViewBag.Groups = _groups;

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

            ViewBag.Templates = _templates;
            return View(domain);
        }

        public async Task<ActionResult> Details(string id)
        {
            ViewBag.Templates = _templates;
            PreAuthDomain domain = await PreAuthDomain.GetDomain(id);
            if (domain.InviteTemplateId.Length > 0)
            {
                domain.InviteTemplateId = string.Format("{0} ({1})", domain.InviteTemplateId, _templates.Where(t => t.Value == domain.InviteTemplateId).Single().Text);
            }
            ViewBag.Operation = "Details";
            return View(domain);
        }

        [HttpPost]
        public async Task<ActionResult> Edit(PreAuthDomain preAuthDomain)
        {
            ViewBag.Templates = _templates;
            if (ModelState.IsValid)
            {
                try
                {
                    preAuthDomain.AuthUser = User.Identity.GetEmail();

                    if (preAuthDomain.Id == null)
                    {
                        preAuthDomain = await PreAuthDomain.AddDomain(preAuthDomain);
                    }
                    else
                    {
                        preAuthDomain = await PreAuthDomain.UpdateDomain(preAuthDomain);
                    }

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