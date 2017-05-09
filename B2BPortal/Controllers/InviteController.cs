using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace B2BPortal.Controllers
{
    public class InviteController : Controller
    {
        // GET: Invite
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Save(IEnumerable<ManualInvite> invitations)
        {
            //todo: process collection of invitations


            return RedirectToAction("Index");
        }
    }

    public class ManualInvite
    {
        public string Email { get; set; }
    }
}