using System;
using B2BPortal.B2B;
using B2BPortal.Data;
using B2BPortal.Infrastructure.Filters;
using B2BPortal.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Mvc;
using B2BPortal.Infrastructure;
using B2BPortal.Rules;
using System.IdentityModel.Claims;

namespace B2BPortal.Areas.Admin.Controllers
{
    [AuthorizedInviter]
    public class HomeController : Controller
    {
        // GET: Admin
        public async Task<ActionResult> Index()
        {
            IEnumerable<GuestRequest> pendingRequests = await DocDBRepo<GuestRequest>.GetItemsAsync(i => i.Disposition == Disposition.Pending && i.DocType == DocTypes.GuestRequest);
            
            return View(pendingRequests);
        }

        [HttpPost]
        public async Task<ActionResult> Approve()
        {
            var approveCount = 0;
            var requestList = await DocDBRepo<GuestRequest>.GetItemsAsync(r => r.Disposition == Disposition.Pending && r.DocType == DocTypes.GuestRequest);
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
                request.Disposition = disposition;
                request.InternalComment = Request.Form[string.Format("InternalComment.{0}", request.Id)];

                //TODO: Upn vs. Email...
                var res = await GuestRequestRules.ExecuteDispositionAsync(request, User.Identity.GetClaim(ClaimTypes.Upn));
            }

            requestList = await DocDBRepo<GuestRequest>.GetItemsAsync(r => r.Disposition == Disposition.Pending && r.DocType == DocTypes.GuestRequest);

            ViewBag.Message = string.Format("{0} {1} approved, invitations sent.", approveCount, Utils.Pluralize(approveCount, "request"));
            return View("Index", requestList);
        }
    }
}