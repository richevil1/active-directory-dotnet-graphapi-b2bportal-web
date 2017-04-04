using AzureB2BInvite.Models;
using B2BPortal.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AzureB2BInvite.AdalUtil;

namespace AzureB2BInvite
{
    /// <summary>
    /// https://developer.microsoft.com/en-us/graph/docs/api-reference/beta/api/invitation_post
    /// </summary>
    public static class InviteManager
    {
        public static async Task<string> SendInvitation(GuestRequest request, string profileUrl, PreAuthDomain domainSettings = null)
        {
            var displayName = string.Format("{0} {1}", request.FirstName, request.LastName);

            var useCustomEmailTemplate = false;
            var redemptionSettings = Settings.SiteRedemptionSettings;

            //use domain custom setting if exists, else use global site config setting
            if (domainSettings != null)
            {
                redemptionSettings = domainSettings.DomainRedemptionSettings;
                //domainSettings.InviteTemplateContent;
                
                if (!string.IsNullOrEmpty(domainSettings.InviteTemplateId))
                {
                    useCustomEmailTemplate = true;
                }
            }

            AdalResponse serverResponse = null;
            try
            {
                // Setup invitation
                var inviteEndPoint = string.Format("{0}/{1}/invitations", Settings.GraphResource, Settings.GraphApiVersion);
                GraphInvitation invitation = new GraphInvitation();
                invitation.InvitedUserDisplayName = displayName;
                invitation.InvitedUserEmailAddress = request.EmailAddress;
                invitation.InviteRedirectUrl = profileUrl;
                invitation.SendInvitationMessage = (!Settings.UseSMTP);

                if (useCustomEmailTemplate && invitation.SendInvitationMessage && domainSettings.InviteTemplateContent.TemplateContent!=null)
                {
                    invitation.InvitedUserMessageInfo = new InvitedUserMessageInfo
                    {
                        CustomizedMessageBody = domainSettings.InviteTemplateContent.TemplateContent
                    };
                }

                // Invite user. Your app needs to have User.ReadWrite.All or Directory.ReadWrite.All to invite
                serverResponse = CallGraph(inviteEndPoint, invitation);
                var responseData = JsonConvert.DeserializeObject<GraphInvitation>(serverResponse.ResponseContent);
                if (responseData.id == null)
                {
                    return string.Format("Error: Invite not sent - API error: {0}", serverResponse.Message);
                }

                if (Settings.UseSMTP)
                {
                    var emailSubject = Settings.InvitationEmailSubject.Replace("{{orgname}}", Settings.InvitingOrganization);

                    string body = FormatEmailBody(responseData, redemptionSettings, domainSettings.InviteTemplateContent);
                    SendViaSMTP(emailSubject, body, invitation.InvitedUserEmailAddress);
                }

                return responseData.Status;
            }
            catch (Exception ex)
            {
                var reason = (serverResponse == null ? "N/A" : serverResponse.ResponseContent);

                return string.Format("Error: {0}<br>Server response: {1}", ex.Message, reason);
            }
        }

        private static string FormatEmailBody(GraphInvitation data, RedemptionSettings redemption, InviteTemplate content)
        {
            var body = content.TemplateContent;
            body = body.Replace("{{InvitingOrgName}}", Settings.InvitingOrganization);
            body = body.Replace("{{InvitationLink}}", data.InviteRedeemUrl);
            body = body.Replace("{{OrgContactEmail}}", redemption.InviterResponseEmailAddr);
            return body;
        }

        private static void SendViaSMTP(string subject, string mailBody, string email)
        {
            MailSender.SendMessage(email, subject, mailBody);
        }
        
        public static IEnumerable<GraphMemberRole> GetDirectoryRoles(string upn)
        {
            AdalResponse serverResponse = null;
            var rolesUri = string.Format("{0}/{1}/users/{2}/memberOf", Settings.GraphResource, Settings.GraphApiVersion, upn);
            serverResponse = CallGraph(rolesUri);
            var list = new List<GraphMemberRole>();
            if (serverResponse.Successful)
            {
                JObject res = JObject.Parse(serverResponse.ResponseContent);
                IList<JToken> roles = res["value"].ToList();
                foreach (var role in roles)
                {
                    var item = JsonConvert.DeserializeObject<GraphMemberRole>(role.ToString());
                    list.Add(item);
                }
            }

            return list;
        }
    }
}