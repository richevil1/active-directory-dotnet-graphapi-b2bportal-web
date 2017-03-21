using B2BPortal.Infrastructure;
using System.Web.Mvc;

namespace B2BPortal.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            if (User.Identity.IsInAnyRole(Settings.InviterRoleNames))
            {
                return RedirectToAction("Index", new  { controller = "Home", action = "Index", area = "Admin" });
            }

            ViewBag.Title = "Request guess access.";
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "About";
            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Contact.";
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