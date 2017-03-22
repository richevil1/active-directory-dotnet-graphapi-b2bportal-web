using B2BPortal.Infrastructure;
using B2BPortal.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace B2BPortal.B2B
{
    public static class InviteManager
    {
        public static async Task<string> SendInvitation(GuestRequest request, string inviteRedirectUrl=null)
        {
            var displayName = string.Format("{0} {1}", request.FirstName, request.LastName);

            string serverResponse = "";
            try
            {
                // Setup invitation
                var inviteEndPoint = string.Format("{0}/{1}/invitations", Settings.GraphResource, Settings.GraphApiVersion);
                GraphInvitation invitation = new GraphInvitation();
                invitation.InvitedUserDisplayName = displayName;
                invitation.InvitedUserEmailAddress = request.EmailAddress;
                invitation.InviteRedirectUrl = (!string.IsNullOrEmpty(inviteRedirectUrl)) ? inviteRedirectUrl : Settings.InviteRedirectUrl;
                invitation.SendInvitationMessage = false;

                // Invite user. Your app needs to have User.ReadWrite.All or Directory.ReadWrite.All to invite
                string InviterUserPrincipalName = Settings.InviterUPN;
                serverResponse = AdalUtil.CallGraph(inviteEndPoint, invitation);
                var responseData = JsonConvert.DeserializeObject<InviteResponse>(serverResponse);
                if (responseData.id == null)
                {
                    var responseError = JsonConvert.DeserializeObject<ResponseError>(serverResponse);
                    return string.Format("Invite not sent - API error: {0}", responseError.code);
                }
                var emailSubject = Settings.InvitationEmailSubject.Replace("{{orgname}}", Settings.InvitingOrganization);

                string body = FormatEmailBody(responseData);
                SendViaSendGrid(emailSubject, body, invitation.InvitedUserEmailAddress);
                return responseData.status;
            }
            catch (Exception ex)
            {
                return string.Format("Error: {0}<br>Server response: {1}", ex.Message, serverResponse);
            }
        }

        private static string FormatEmailBody(InviteResponse data)
        {
            var body = new StringBuilder();
            body.AppendFormat("You've been invited to access applications in the {0} organization<br>", Settings.InvitingOrganization);
            body.AppendFormat("by {0}<br>", Settings.InviterUPN);
            body.AppendFormat("<a href='{0}'>Get Started</a><br>", data.inviteRedeemUrl);
            body.AppendFormat("Return to the above link at any time for access.<br><hr>");
            body.AppendFormat("Questions? Contact {0} at <a href='mailto:{1}'>{1}</a>", Settings.InvitingOrganization, Settings.InviterUPN);
            return body.ToString();
        }

        //private static string SendViaGraph(string subject, string mailBody, string email)
        //{
        //    // Create mail. Your app needs Mail.Send scope to send.
        //    var emailEndPoint = string.Format("{0}/beta/users/{1}/sendMail", Settings.GraphResource, Settings.InviterUPN);
        //    GraphEmail mail = new GraphEmail();
        //    mail.message = new Message();
        //    mail.message.Subject = subject;
        //    mail.message.Body = new ItemBody();
        //    mail.message.Body.Content = mailBody;
        //    Recipient recipient = new Recipient();
        //    recipient.EmailAddress = new Microsoft.Graph.EmailAddress();
        //    recipient.EmailAddress.Address = email;
        //    mail.recipients = new List<Recipient>();
        //    mail.recipients.Add(recipient);

        //    // Send email.
        //    var content = new StringContent(JsonConvert.SerializeObject(mail));
        //    content.Headers.Add("ContentType", "application/json");
        //    using (var httpClient = new HttpClient())
        //    {
        //        var postResponse = httpClient.PostAsync(emailEndPoint, content).Result;
        //        var serverResponse = postResponse.Content.ReadAsStringAsync().Result;
        //        return serverResponse;
        //    }
        //}

        private static void SendViaSendGrid(string subject, string mailBody, string email)
        {
            MailSender.SendMessage(email, subject, mailBody);
        }
        public static IEnumerable<GraphMemberRole> GetDirectoryRoles(string upn)
        {
            string serverResponse = "";
            try
            {
                var rolesUri = string.Format("{0}/{1}/users/{2}/memberOf", Settings.GraphResource, Settings.GraphApiVersion, upn);
                serverResponse = AdalUtil.CallGraph(rolesUri);

                JObject res = JObject.Parse(serverResponse);
                IList<JToken> roles = res["value"].ToList();
                var list = new List<GraphMemberRole>();
                foreach(var role in roles)
                {
                    var item = JsonConvert.DeserializeObject<GraphMemberRole>(role.ToString());
                    list.Add(item);
                }

                return list;
            }
            catch (Exception ex)
            {
                Logging.WriteToAppLog(string.Format("Error getting roles. ServerResponse: {0}", serverResponse), System.Diagnostics.EventLogEntryType.Error, ex);
                throw;
            }
        }
    }
}