using Microsoft.Azure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using B2BPortal.Models;
using Microsoft.Graph;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Newtonsoft.Json;

namespace B2BPortal.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            // has the user just signed up?
            string strFormAction = this.Request["submitButton"];
            if (strFormAction == "userSignUp")
            {
                // collect form data
                string firstName = this.Request["FirstName"];
                string lastName = this.Request["LastName"];
                string displayName = firstName + " " + lastName;
                string emailAddress = this.Request["EmailAddress"];
                string department = this.Request["Department"];
                // create table if it doesn't exist
                const string TABLENAME = "TableRequests";
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageConnectionString"));
                CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
                CloudTable requestTable = tableClient.GetTableReference(TABLENAME);
                requestTable.CreateIfNotExists();
                // add new entity to table (no concurrency checks for now)
                RequestEntity newRequest = new RequestEntity(displayName, emailAddress, department);
                TableOperation insert = TableOperation.InsertOrReplace(newRequest);
                requestTable.Execute(insert);
            }
            ViewBag.Message = "Sign up for access.";
            return View();
        }
        public async Task<ActionResult> About()
        {
            // has the user approved or rejected the list of approvals?
            string strFormAction = this.Request["submitButton"];
            // get the list of requests
            const string TABLENAME = "TableRequests";
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageConnectionString"));
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            CloudTable requestTable = tableClient.GetTableReference(TABLENAME);
            requestTable.CreateIfNotExists();
            var entities = requestTable.ExecuteQuery(new TableQuery<RequestEntity>()).ToList();
            foreach (RequestEntity entity in entities)
            {
                if (strFormAction == "requestApprove")
                {
                    // send the invitation
                    string strResponse = await SendInvitation(entity.RowKey, entity.Email);
                    // update status on existing request (concurrency issue if item has been removed)
                    entity.Status = strResponse;
                    TableOperation update = TableOperation.InsertOrMerge(entity);
                    requestTable.Execute(update);
                }
                else if (strFormAction == "requestReject")
                {
                    // remove the request
                    TableOperation deleteOperation = TableOperation.Delete(entity);
                    requestTable.Execute(deleteOperation);
                }
            }

            ViewBag.Message = "Approve requests.";
            return View();
        }
        public ActionResult Contact()
        {
            // has the user just edited their profile?
            string strFormAction = this.Request["submitButton"];
            if (strFormAction == "userUpdate")
            {
                // collect form data
                string firstName = this.Request["FirstName"];
                string lastName = this.Request["LastName"];
                string displayName = firstName + " " + lastName;
                string emailAddress = this.Request["EmailAddress"];
                string department = this.Request["Department"];
                // create table if it doesn't exist
                const string TABLENAME = "TableRequests";
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageConnectionString"));
                CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
                CloudTable requestTable = tableClient.GetTableReference(TABLENAME);
                requestTable.CreateIfNotExists();
                // add new entity to table (no concurrency checks for now)
                RequestEntity newRequest = new RequestEntity(displayName, emailAddress, department);
                TableOperation insert = TableOperation.InsertOrReplace(newRequest);
                requestTable.Execute(insert);
            }
            ViewBag.Message = "Edit profile.";
            return View();
        }

        private async Task<AuthenticationResult> Authenticate()
        {
            string AADInstance = CloudConfigurationManager.GetSetting("ida:AADInstance");
            string TenantID = CloudConfigurationManager.GetSetting("ida:TenantId");
            string AppClientId = CloudConfigurationManager.GetSetting("ida:ClientId");
            string AppClientSecret = CloudConfigurationManager.GetSetting("ida:ClientSecret");
            string GraphResource = "https://graph.microsoft.com";

            AuthenticationContext authContext = new AuthenticationContext(string.Format(AADInstance, TenantID));
            try
            {
                AuthenticationResult authResult = await authContext.AcquireTokenAsync(GraphResource, new ClientCredential(AppClientId, AppClientSecret));
                return authResult;
            }
            catch (AdalException)
            {
                return null;
            }
        }
        private async Task<string> SendInvitation(string displayName, string email)
        {
            // Get auth token
            AuthenticationResult authResult = await Authenticate();
            string accessToken = authResult.AccessToken;

            // Setup http client
            HttpClient httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromSeconds(300);
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            httpClient.DefaultRequestHeaders.Add("client-request-id", Guid.NewGuid().ToString());
            httpClient.DefaultRequestHeaders.GetValues("client-request-id").Single();

            // Setup invitation
            var inviteEndPoint = string.Format("https://graph.microsoft.com/beta/invitations");
            Invitation invitation = new Invitation();
            invitation.InvitedUserDisplayName = displayName;
            invitation.InvitedUserEmailAddress = email;
            invitation.InviteRedirectUrl = "https://www.microsoft.com";
            invitation.SendInvitationMessage = false;

            // Invite user. Your app needs to have User.ReadWrite.All or Directory.ReadWrite.All to invite
            string InviterUserPrincipalName = CloudConfigurationManager.GetSetting("ida:InviterUPN");
            HttpContent content = new StringContent(JsonConvert.SerializeObject(invitation));
            content.Headers.Add("ContentType", "application/json");
            var postResponse = httpClient.PostAsync(inviteEndPoint, content).Result;
            string serverResponse = postResponse.Content.ReadAsStringAsync().Result;
            // Create mail. Your app needs Mail.Send scope to send.
            var emailEndPoint = string.Format("https://graph.microsoft.com/beta/users/{0}/sendMail", InviterUserPrincipalName);
            Email mail = new Email();
            mail.message = new Message();
            mail.message.Subject = "Inviation Email";
            mail.message.Body = new ItemBody();
            mail.message.Body.Content = serverResponse;
            Recipient recipient = new Recipient();
            recipient.EmailAddress = new EmailAddress();
            recipient.EmailAddress.Address = invitation.InvitedUserEmailAddress;
            mail.recipients = new List<Recipient>();
            mail.recipients.Add(recipient);
            // Send email.
            content = new StringContent(JsonConvert.SerializeObject(mail));
            content.Headers.Add("ContentType", "application/json");
            postResponse = httpClient.PostAsync(emailEndPoint, content).Result;
            serverResponse = postResponse.Content.ReadAsStringAsync().Result;
            return serverResponse;
        }
    }
    public class Invitation
    {
        public string InvitedUserDisplayName { get; set; }
        public string InvitedUserEmailAddress { get; set; }
        public bool SendInvitationMessage { get; set; }
        public string InviteRedirectUrl { get; set; }
    }
    public class Email
    {
        public Message message;
        public List<Recipient> recipients;
    }
}