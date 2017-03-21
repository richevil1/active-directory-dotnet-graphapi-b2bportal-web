using B2BPortal.B2B;
using B2BPortal.Data;
using B2BPortal.Infrastructure.Filters;
using B2BPortal.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Mvc;

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
            // get the list of requests
            var requestList = await DocDBRepo<GuestRequest>.GetItemsAsync(r => r.Disposition==Disposition.Pending && r.DocType == DocTypes.GuestRequest);

            //var response = StorageTableRepo.GetRequestList();
            foreach (var request in requestList)
            {
                //for each pending request from the table, find the match in the submitted items
                //and perform the requested operation.
                //(re-looping from the table for security)


            }

            ViewBag.Message = "Approve requests.";
            return View("Index");
        }

        //[HttpPost]
        //public async Task<ActionResult> Approve()
        //{
        //    // has the user approved or rejected the list of approvals?
        //    string strFormAction = this.Request["submitButton"];
        //    // get the list of requests
        //    var response = StorageTableRepo.GetRequestList();
        //    foreach (RequestEntity entity in response.ListItems)
        //    {
        //        if (strFormAction == "requestApprove")
        //        {
        //            // send the invitation
        //            //todo - duh
        //            string strResponse = await InviteManager.SendInvitation(null);

        //            // update status on existing request (concurrency issue if item has been removed)
        //            entity.Status = strResponse;
        //            StorageTableRepo.UpdateEntity(response.RequestTable, entity);
        //        }
        //        else if (strFormAction == "requestReject")
        //        {
        //            // remove the request
        //            StorageTableRepo.DeleteEntity(response.RequestTable, entity);
        //        }
        //    }

        //    ViewBag.Message = "Approve requests.";
        //    return View("Index");
        //}
    }
}