using AzureB2BInvite;
using B2BPortal.Infrastructure;
using B2BPortal.Models;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace B2BPortal.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            if (User.Identity.IsInAnyRole(AdalUtil.Settings.InviterRoleNames))
            {
                return RedirectToAction("Index", new { controller = "Home", action = "Index", area = "Admin" });
            }

            if (!Settings.SiteConfigReady)
            {
                return View("NoConfig");
            }

            ViewBag.Title = string.Format("Request guest access to the {0} org", AdalUtil.Settings.InvitingOrganization);
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "About";
            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Contact";
            return View();
        }

        public async Task<ActionResult> TOS(string configId = null)
        {
            if (configId != null)
            {
                var config = await SiteConfig.GetConfig(configId);
                ViewBag.TOSContent = config.TOSDocument;
            }
            else
            {
                ViewBag.TOSContent = Settings.CurrSiteConfig.TOSDocument;
            }
            return View();
        }

        public ActionResult Error()
        {
            return View();
        }

        public ActionResult Claims()
        {
            return View();
        }
    }
}