using AzureB2BInvite;
using AzureB2BInvite.Models;
using AzureB2BInvite.Rules;
using System.Linq;
using B2BPortal.Infrastructure;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace B2BPortal.Controllers
{
    public class ProfileController : Controller
    {
        [Authorize]
        // GET: Profile
        public async Task<ActionResult> Index()
        {
            var user = ProfileManager.GetUserProfile(User.Identity.Name);
            ViewBag.RedirectLink = await ProfileManager.GetRedirUrl(User.Identity.Name);
            
            ViewBag.Message = "Edit profile";
            return View("Index", user);
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<ActionResult> SignUp(GuestRequest request)
        {
            //sanity check: not trusting the passed-in preauth setting
            request.PreAuthed = (User.Identity.IsAuthenticated && User.Identity.GetClaim(CustomClaimTypes.TenantId) != AdalUtil.Settings.TenantID);

            var result = await GuestRequestRules.SignUpAsync(request);

            return View(result);
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult> Update(AADUserProfile user)
        {
            if (ModelState.IsValid)
            {
                var redirectLink = await ProfileManager.GetRedirUrl(User.Identity.Name);

                var orgUser = ProfileManager.GetUserProfile(User.Identity.Name);
                var data = AADUserProfile.GetDeltaChanges(orgUser, user);

                ProfileManager.UpdateProfile(data, orgUser.UserPrincipalName);
                return Redirect(redirectLink);
            }

            ViewBag.Message = "Edit profile";

            return View("Index", user);
        }
    }
}