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
            var user = ProfileManager.GetUserProfile(User.Identity.GetClaim(CustomClaimTypes.ObjectIdentifier));
            ViewBag.RedirectLink = await ProfileManager.GetRedirUrl(User.Identity.Name);
            ViewBag.Message = "Edit profile";
            return View("Index", user);
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<ActionResult> SignUp(GuestRequest request)
        {
            //sanity check: not trusting the passed-in preauth setting
            request.PreAuthed = (User.Identity.GetClaim(CustomClaimTypes.AuthType) == AuthTypes.B2EMulti);

            var result = await GuestRequestRules.SignUpAsync(request, Utils.GetProfileUrl(Request));

            return View(result);
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult> Update(AADUserProfile user)
        {
            if (ModelState.IsValid)
            {
                var oid = User.Identity.GetClaim(CustomClaimTypes.ObjectIdentifier);
                var redirectLink = await ProfileManager.GetRedirUrl(User.Identity.Name);
                if (redirectLink == null) redirectLink = "/Profile";

                var orgUser = ProfileManager.GetUserProfile(oid);
                var data = AADUserProfile.GetDeltaChanges(orgUser, user);

                ProfileManager.UpdateProfile(data, oid);
                return Redirect(redirectLink);
            }

            ViewBag.Message = "Edit profile";

            return View("Index", user);
        }

        public ActionResult RemoteProfile()
        {
            return View();
        }
    }
}