using System;
using B2BPortal.Data;
using B2BPortal.Infrastructure.Filters;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Mvc;
using B2BPortal.Infrastructure;
using System.IdentityModel.Claims;
using AzureB2BInvite.Models;
using B2BPortal.Interfaces;
using AzureB2BInvite.Rules;

namespace B2BPortal.Areas.Admin.Controllers
{
    [AuthorizedInviter]
    public class HomeController : Controller
    {
        // GET: Admin
        public async Task<ActionResult> Index()
        {
            if (!Settings.SiteConfigReady)
            {
                return RedirectToAction("Index", new { controller = "SiteConfig", action = "Index", area = "Admin" });
            }

            IEnumerable<GuestRequest> pendingRequests = await DocDBRepo.DB<GuestRequest>.GetItemsAsync(i => i.Disposition == Disposition.Pending);
            
            return View(pendingRequests);
        }

        [HttpPost]
        public async Task<ActionResult> Approve()
        {
            var approveCount = 0;
            var deniedCount = 0;
            var requestList = await GuestRequestRules.GetPendingRequestsAsync();

            foreach (var request in requestList)
            {
                var strDisposition = Request.Form[string.Format("Disposition.{0}", request.Id)];
                var disposition = (Disposition)Enum.Parse(typeof(Disposition), strDisposition);
                if (disposition == Disposition.Pending)
                {
                    continue;
                }
                if (disposition == Disposition.Approved)
                {
                    approveCount++;
                }
                if (disposition == Disposition.Denied)
                {
                    deniedCount++;
                }
                request.Disposition = disposition;
                request.InternalComment = Request.Form[string.Format("InternalComment.{0}", request.Id)];

                //TODO: Upn vs. Email...
                var res = await GuestRequestRules.ExecuteDispositionAsync(request, User.Identity.Name);
            }

            requestList = await GuestRequestRules.GetPendingRequestsAsync();

            ViewBag.Message = string.Format("{0} {1} approved, {2} {3} denied, invitations sent.", approveCount, Utils.Pluralize(approveCount, "request"), deniedCount, Utils.Pluralize(deniedCount, "request"));
            return View("Index", requestList);
        }
    }
}