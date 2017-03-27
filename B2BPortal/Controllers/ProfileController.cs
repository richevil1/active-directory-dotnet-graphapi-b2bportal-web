using AzureB2BInvite;
using AzureB2BInvite.Models;
using AzureB2BInvite.Rules;
using B2BPortal.Infrastructure;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace B2BPortal.Controllers
{
    public class ProfileController : Controller
    {
        [Authorize]
        // GET: Profile
        public ActionResult Index()
        {
            var user = ProfileManager.GetUserProfile(User.Identity.Name);
            ViewBag.Message = "Edit profile.";
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
        public ActionResult Update(AADUserProfile user)
        {
            if (ModelState.IsValid)
            {
                var orgUser = ProfileManager.GetUserProfile(User.Identity.Name);
                var data = AADUserProfile.GetDeltaChanges(orgUser, user);

                ProfileManager.UpdateProfile(data, orgUser.UserPrincipalName);
                ViewBag.UpdateStatus = "Profile updated successfully";
                ViewBag.Message = "Edit profile";
            }
            return Redirect("https://myapps.microsoft.com");
        }
    }
}