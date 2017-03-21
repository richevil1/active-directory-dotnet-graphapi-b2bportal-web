using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace B2BPortal.Models
{
    public class GuestRequest: DocModelBase
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

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
        /// What was the result of the approval review?
        /// </summary>
        [JsonProperty(PropertyName = "disposition")]
        public Disposition Disposition { get; set; }

        public void Init()
        {
            Disposition = Disposition.Pending;
            Id = Guid.NewGuid().ToString();
            Disposition = Disposition.Pending;
            PreAuthed = false;
            RequestDate = DateTime.UtcNow;
            LastModDate = DateTime.UtcNow;
        }
    }
}
