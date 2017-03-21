using B2BPortal.Data;
using B2BPortal.Infrastructure.Filters;
using B2BPortal.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace B2BPortal.Areas.Admin.Controllers
{
    [AuthorizedInviter]
    public class PreApprovalController : Controller
    {
        public async Task<ActionResult> Index()
        {
            IEnumerable<PreAuthDomain> domains = await DocDBRepo<PreAuthDomain>.GetItemsAsync(d => d.DocType==DocTypes.PreAuthDomains);
            return View(domains);
        }

        public ActionResult Create()
        {
            ViewBag.Operation = "Create";
            return View("Edit");
        }

        [HttpPost]
        public async Task<ActionResult> Create(PreAuthDomain domain)
        {
            try
            {
                domain.Init();
                var doc = await DocDBRepo<PreAuthDomain>.CreateItemAsync(domain);
                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        public async Task<ActionResult> Edit(string id)
        {
            PreAuthDomain domain = await DocDBRepo<PreAuthDomain>.GetItemAsync(id);
            ViewBag.Operation = "Edit";
            return View(domain);
        }

        [HttpPost]
        public async Task<ActionResult> Edit(PreAuthDomain domain)
        {
            try
            {
                domain.Groups = new List<string>(domain.GroupsList.Split(','));
                var doc = await DocDBRepo<PreAuthDomain>.UpdateItemAsync(domain.Id, domain);

                return RedirectToAction("Index");
            }
            catch
            {
                ViewBag.Operation = "Edit";
                return View();
            }
        }

        public ActionResult Delete(int id)
        {
            ViewBag.Operation = "Delete";
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Delete(PreAuthDomain domain)
        {
            try
            {
                var doc = await DocDBRepo<PreAuthDomain>.DeleteItemAsync(domain.Id, domain);

                return RedirectToAction("Index");
            }
            catch
            {
                ViewBag.Operation = "Delete";
                return View();
            }
        }
    }
}