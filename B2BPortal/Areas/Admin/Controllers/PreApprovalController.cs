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
using Microsoft.Graph;

namespace B2BPortal.Areas.Admin.Controllers
{
    [AuthorizedInviter]
    public class PreApprovalController : AsyncController
    {
        private List<InviteTemplate> _templates;
        private List<Group> _groups;

        public PreApprovalController()
        {
            var task = Task.Run(async () => {
                _templates = (await TemplateUtilities.GetTemplates()).ToList();
                _groups = (await new GraphUtil().GetGroups()).ToList();
            });
            task.Wait();
        }
        private IEnumerable<SelectListItem> GetTemplates(string templateId)
        {
            var res = new List<SelectListItem>();
            res.AddRange(_templates.Select(t => new SelectListItem { Selected = (t.Id==templateId), Text = t.TemplateName, Value = t.Id }));
            return res;
        }

        private IEnumerable<SelectListItem> GetGroups(IEnumerable<string> groupIds)
        {
            var res = new List<SelectListItem>();
            res.AddRange(_groups.Select(g => new SelectListItem { Selected = (groupIds.Any(i => i == g.Id)), Text = g.DisplayName, Value = g.Id }));
            return res;
        }

        public async Task<ActionResult> Index()
        {
            IEnumerable<PreAuthDomain> domains = await PreAuthDomain.GetDomains();
            return View(domains);
        }

        [HttpPost]
        public async Task<ActionResult> Create(PreAuthDomain domain)
        {
            ViewBag.Templates = GetTemplates(domain.InviteTemplateId);
            ViewBag.Groups = GetGroups(domain.Groups);

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
                //set default redirect URL
                domain.DomainRedemptionSettings.InviteRedirectUrl = string.Format("https://myapps.microsoft.com/{0}", AdalUtil.Settings.Tenant);
            }
            else
            {
                ViewBag.Operation = "Edit";
                domain = await PreAuthDomain.GetDomain(id);
            }

            ViewBag.Templates = GetTemplates(domain.InviteTemplateId);
            ViewBag.Groups = GetGroups(domain.Groups);
            return View(domain);
        }

        [HttpPost]
        public async Task<ActionResult> Edit(PreAuthDomain domain)
        {
            ViewBag.Templates = GetTemplates(domain.InviteTemplateId);
            ViewBag.Groups = GetGroups(domain.Groups);

            if (ModelState.IsValid)
            {
                try
                {
                    domain.AuthUser = User.Identity.GetEmail();

                    if (domain.Id == null)
                    {
                        domain = await PreAuthDomain.AddDomain(domain);
                    }
                    else
                    {
                        domain = await PreAuthDomain.UpdateDomain(domain);
                    }

                    return RedirectToAction("Index");
                }
                catch
                {
                    ViewBag.Operation = "Edit";
                    return View(domain);
                }
            }
            return View(domain);
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