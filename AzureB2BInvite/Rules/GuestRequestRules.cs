using AzureB2BInvite.Models;
using B2BPortal.Data;
using B2BPortal.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace AzureB2BInvite.Rules
{
    public static class GuestRequestRules
    {
        public static async Task<GuestRequest> SignUpAsync(GuestRequest request, string profileUrl)
        {
            request.Init();
            var doc = await DocDBRepo.DB<GuestRequest>.CreateItemAsync(request);
            
            if (request.PreAuthed)
            {
                //check to see if this domain has been approved for pre-authentication
                var approvedDomainSettings = await GetMatchedDomain(request.EmailAddress);
                if (approvedDomainSettings != null)
                {
                    request.Disposition = Disposition.AutoApproved;
                    request = await ExecuteDispositionAsync(request, approvedDomainSettings.AuthUser, profileUrl, approvedDomainSettings);
                }
            }

            return request;
        }
        public static async Task<PreAuthDomain> GetMatchedDomain(string email)
        {
            var domain = email.Split('@')[1];
            return (await DocDBRepo.DB<PreAuthDomain>.GetItemsAsync(d => d.DomainName == domain)).SingleOrDefault();
        }

        public static async Task<GuestRequest> ExecuteDispositionAsync(GuestRequest request, string approver, string profileUrl, PreAuthDomain domainSettings = null)
        {
            request.AuthUser = approver;
            request.LastModDate = DateTime.UtcNow;
           
            if (request.Disposition == Disposition.Approved || request.Disposition == Disposition.AutoApproved)
            {
                //INVITE
                request.Status = await InviteManager.SendInvitation(request, profileUrl, domainSettings);
                if (request.Status.Substring(0, 5) == "Error")
                {
                    request.Disposition = Disposition.Pending;
                }
            }

            //UPDATE
            await DocDBRepo.DB<GuestRequest>.UpdateItemAsync(request);
            return request;
        }

        public static async Task<PreAuthDomain> GetPreauthDomain(string domainName)
        {
            return (await DocDBRepo.DB<PreAuthDomain>.GetItemsAsync(d => d.DomainName == domainName)).SingleOrDefault();
        }

        public static async Task<GuestRequest> GetUserAsync(string email)
        {
            return (await DocDBRepo.DB<GuestRequest>.GetItemsAsync(r => r.EmailAddress == email)).SingleOrDefault();
        }

        public static async Task<IEnumerable<GuestRequest>> GetPendingRequestsAsync()
        {
            return await DocDBRepo.DB<GuestRequest>.GetItemsAsync(r => r.Disposition == Disposition.Pending);
        }
    }
}