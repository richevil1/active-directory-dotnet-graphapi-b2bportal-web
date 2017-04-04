using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using B2BPortal.Infrastructure;
using B2BPortal.Models;
using B2BPortal.Infrastructure.Filters;
using AzureB2BInvite.Models;

namespace B2BPortal.Areas.Admin.Controllers
{
    [AuthorizedInviter]
    public class SiteConfigController : Controller
    {
        public async Task<ActionResult> Index()
        {
            var config = await SiteConfig.GetCurrConfig();
            if (config == null)
            {
                config = new SiteConfig();
            }
            return View("Edit", config);
            //return View(config);
        }

        public async Task<ActionResult> List()
        {
            var configs = await SiteConfig.GetAllConfigs();
            return View(configs);
        }

        public async Task<ActionResult> Details(string id)
        {
            var config = await SiteConfig.GetConfig(id);
            return View(config);
        }

        public async Task<ActionResult> Edit(string id)
        {
            ViewBag.Operation = "Edit";
            var config = await SiteConfig.GetConfig(id);
            return View(config);
        }

        public async Task<ActionResult> HistoryDetail(string id)
        {
            var config = await SiteConfig.GetConfig(id);
            return View(config);
        }

        [HttpPost]
        public async Task<ActionResult> Save(SiteConfig config)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    config.ConfigAuthor = User.Identity.GetEmail();
                    config = await SiteConfig.SetNewConfig(config);
                    return RedirectToAction("Index");
                }
                catch
                {
                    ViewBag.Error = "An error occured saving your config, please check the error logs and try again.";
                    return View("Edit", config);
                }
            }
            return View("Edit", config);
        }
    }
}
