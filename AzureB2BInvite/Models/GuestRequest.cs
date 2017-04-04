using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using B2BPortal.Interfaces;

namespace AzureB2BInvite.Models
{
    public class GuestRequest: DocModelBase, IDocModelBase
    {
        /// <summary>
        /// Email address of the guest requester
        /// </summary>
        [JsonProperty(PropertyName = "emailAddress")]
        public string EmailAddress { get; set; }

        /// <summary>
        /// First Name address of the guest requester
        /// </summary>
        [JsonProperty(PropertyName = "firstName")]
        public string FirstName { get; set; }

        /// <summary>
        /// Last Name address of the guest requester
        /// </summary>
        [JsonProperty(PropertyName = "lastName")]
        public string LastName { get; set; }

        /// <summary>
        /// The returned status of the B2B invite API request
        /// </summary>
        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; }

        /// <summary>
        /// Date the request was made
        /// </summary>
        [JsonProperty(PropertyName = "requestDate")]
        public DateTime RequestDate { get; set; }

        /// <summary>
        /// Last record update
        /// </summary>
        [JsonProperty(PropertyName = "lastModDate")]
        public DateTime LastModDate { get; set; }

        /// <summary>
        /// The UPN of the user updating (approving or denying) this Guest request (overwritted on subsequent edits
        /// to reflect the last user reviewing)
        /// </summary>
        [JsonProperty(PropertyName = "authUser")]
        public string AuthUser { get; set; }

        /// <summary>
        /// Optional guest requester comments
        /// </summary>
        [JsonProperty(PropertyName = "comment")]
        public string Comment { get; set; }

        /// <summary>
        /// Optional internal comment by the approver
        /// </summary>
        [JsonProperty(PropertyName = "internalComment")]
        public string InternalComment { get; set; }

        /// <summary>
        /// Was the user authenticated when making their request (multi-tenant)?
        /// </summary>
        [JsonProperty(PropertyName = "preAuthed")]
        public bool PreAuthed { get; set; }

        /// <summary>
        /// Version of site configuration settings (including Terms of Service) in effect when user agreed to TOS
        /// </summary>
        [JsonProperty(PropertyName = "siteConfigId")]
        public string SiteConfigId { get; set; }

        /// <summary>
        /// Did the user check the box to agree with the TOS?
        /// </summary>
        [JsonProperty(PropertyName = "tosAgreed")]
        public bool TOSAgreed { get; set; }

        /// <summary>
        /// What was the result of the approval review?
        /// </summary>
        [JsonProperty(PropertyName = "disposition")]
        public Disposition Disposition { get; set; }

        /// <summary>
        /// For a new request, pre-populate all the defaults.
        /// </summary>
        public void Init()
        {
            Disposition = Disposition.Pending;
            RequestDate = DateTime.UtcNow;
            LastModDate = DateTime.UtcNow;
        }
    }
}
