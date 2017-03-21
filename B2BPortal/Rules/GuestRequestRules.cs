using B2BPortal.B2B;
using B2BPortal.Data;
using B2BPortal.Models;
using System.Linq;
using System.Threading.Tasks;

namespace B2BPortal.Rules
{
    public static class GuestRequestRules
    {
        public static async Task<GuestRequest> SignUpAsync(GuestRequest request)
        {
            request.Init();
            var doc = await DocDBRepo<GuestRequest>.CreateItemAsync(request);
            
            if (request.PreAuthed)
            {
                var domain = request.EmailAddress.Split('@')[1];

                //check to see if this domain has been approved for pre-authentication
                var approvedDomainSettings = (await DocDBRepo<PreAuthDomain>.GetItemsAsync(d => d.Domain == domain && d.DocType==DocTypes.PreAuthDomains)).SingleOrDefault();
                if (approvedDomainSettings != null)
                {
                    //perform the guest invite, then update the record
                    //todo: wrap in a tx?

                    //INVITE
                    request.Status = await InviteManager.SendInvitation(request);

                    //UPDATE
                    request.AuthUser = approvedDomainSettings.AuthUser;
                    request.Disposition = Disposition.AutoApproved;
                }
            }

            await DocDBRepo<GuestRequest>.UpdateItemAsync(request.Id, request);
            return request;
        }
        public static async Task<GuestRequest> ExecuteDispositionAsync(GuestRequest request, string approver)
        {
            request.Init();
            var doc = await DocDBRepo<GuestRequest>.CreateItemAsync(request);

            if (request.Disposition==Disposition.Approved)
            {
                //INVITE
                request.Status = await InviteManager.SendInvitation(request);

                //UPDATE
                request.AuthUser = approver;
                await DocDBRepo<GuestRequest>.UpdateItemAsync(request.Id, request);
            }
            return request;
        }
    }
}