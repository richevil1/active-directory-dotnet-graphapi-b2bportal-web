using B2BPortal.Infrastructure;
using B2BPortal.Models;
using B2BPortal.Rules;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
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
            ViewBag.Message = "Edit profile.";
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<ActionResult> SignUp(GuestRequest request)
        {
            //not trusting the passed-in preauth setting
            request.PreAuthed = (User.Identity.IsAuthenticated && User.Identity.GetClaim(CustomClaimTypes.TenantId) != Settings.TenantID);

            var result = await GuestRequestRules.SignUpAsync(request);

            return View(result);
        }

        [Authorize]
        [HttpPost]
        public ActionResult Update()
        {
            string strFormAction = this.Request["submitButton"];
            //if (strFormAction == "userUpdate")
            //{
                // collect form data
                string firstName = this.Request["FirstName"];
                string lastName = this.Request["LastName"];
                string displayName = firstName + " " + lastName;
                string emailAddress = this.Request["EmailAddress"];
                string department = this.Request["Department"];
                // create table if it doesn't exist
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(Settings.StorageConnectionString);
                CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
                CloudTable requestTable = tableClient.GetTableReference(Settings.RequestsTableName);
                requestTable.CreateIfNotExists();
                // add new entity to table (no concurrency checks for now)
                //RequestEntity newRequest = new RequestEntity(displayName, emailAddress, department);
                //TableOperation insert = TableOperation.InsertOrReplace(newRequest);
                //requestTable.Execute(insert);
            //}

            ViewBag.Message = "Edit profile.";
            return View("Index");
        }
    }
}