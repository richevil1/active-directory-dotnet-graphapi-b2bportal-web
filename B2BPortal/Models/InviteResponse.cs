using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace B2BPortal.Models
{

    public class EmailAddress
    {
        public string name { get; set; }
        public string address { get; set; }
    }

    public class CcRecipient
    {
        public EmailAddress emailAddress { get; set; }
    }

    public class InvitedUserMessageInfo
    {
        public string messageLanguage { get; set; }
        public List<CcRecipient> ccRecipients { get; set; }
        public string customizedMessageBody { get; set; }
    }

    public class InvitedUser
    {
        public string id { get; set; }
    }

    public class InviteResponse
    {
        [JsonProperty(PropertyName = "@odata.context") ]
        public string context { get; set; }
        public string id { get; set; }
        public string inviteRedeemUrl { get; set; }
        public string invitedUserDisplayName { get; set; }
        public string invitedUserType { get; set; }
        public string invitedUserEmailAddress { get; set; }
        public bool sendInvitationMessage { get; set; }
        public InvitedUserMessageInfo invitedUserMessageInfo { get; set; }
        public string inviteRedirectUrl { get; set; }
        public string status { get; set; }
        public InvitedUser invitedUser { get; set; }
    }

    //error object
    public class ResponseError
    {
        public string code;
        public string message;
        public InnerError innerError;
    }
    public class InnerError
    {
        [JsonProperty(PropertyName = "request-id")]
        public string requestId;
        public string data;
    }
}

/*
{
	"@odata.context": "https://graph.microsoft.com/beta/$metadata#invitations/$entity",
	"id": "5cfe5c91-e152-44d0-bb6a-4ba778e70ce8",
	"inviteRedeemUrl": "https://invitations.microsoft.com/redeem/?tenant=a70fadca-e867-489c-b119-72dc9f00c26b&user=5cfe5c91-e152-44d0-bb6a-4ba778e70ce8&ticket=lDDvmUN1PRuBL9BCVYDLoi1fQGcE3RxKvX7g22bTknw%3d&lc=1033&ver=2.0",
	"invitedUserDisplayName": "Brett Hacker",
	"invitedUserType": "Guest",
	"invitedUserEmailAddress": "bhacker@thehacker.com",
	"sendInvitationMessage": false,
	"invitedUserMessageInfo": {
		"messageLanguage": null,
		"ccRecipients": [{
			"emailAddress": {
				"name": null,
				"address": null
			}
		}],
		"customizedMessageBody": null
	},
	"inviteRedirectUrl": "http://www.microsoft.com/",
	"status": "PendingAcceptance",
	"invitedUser": {
		"id": "a7431475-af93-45dd-8290-1d0a3b891fbc"
	}
}

{
  "error": {
    "code": "UnknownError",
    "message": "",
    "innerError": {
      "request-id": "58aa608e-72b6-4333-8005-948f28696520",
      "date": "2017-03-16T05:19:36"
    }
  }
}

*/
