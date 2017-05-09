using AzureB2BInvite;
using AzureB2BInvite.Rules;
using Microsoft.Graph;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;

namespace B2BPortal.api
{
    public class PreauthController : ApiController
    {
        [HttpPost]
        public async Task<bool> CheckDomain(PreAuthReq req)
        {
            var domainName = req.Email.Split('@')[1];
            var domain = await GuestRequestRules.GetPreauthDomain(domainName);
            return (domain != null);
        }

        [HttpGet]
        public bool GetPublicTenant(string id)
        {
            var res = AdalUtil.FindPublicAADTenant(id);
            return (res.Error == null);
        }

        [HttpGet]
        public async Task<IEnumerable<dynamic>> GetAADGroupList(string filter)
        {
            var groups = await new GraphUtil().GetGroups(filter);
            return groups.Select(g => new { g.DisplayName, g.Id });
        }

    }
    public class PreAuthReq
    {
        public string Email { get; set; }
        public string DomainName { get; set; }
    }
}
