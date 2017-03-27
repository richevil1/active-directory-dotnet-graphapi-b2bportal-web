using B2BPortal.Data;
using B2BPortal.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AzureB2BInvite.Models
{
    public class PreAuthDomain: DocModelBase, IDocModelBase
    {
        /// <summary>
        /// The UPN of the user creating this PreAuth record
        /// </summary>
        [DisplayName("Auth User")]
        [ScaffoldColumn(false)]
        [JsonProperty(PropertyName = "authUser")]
        public string AuthUser { get; set; }

        /// <summary>
        /// Date this PreAuth record was created
        /// </summary>
        [DisplayName("Create Date")]
        [ScaffoldColumn(false)]
        [JsonProperty(PropertyName = "createDate")]
        public DateTime CreateDate { get; set; }

        /// <summary>
        /// Date this PreAuth record was updated
        /// </summary>
        [DisplayName("Last Updated")]
        [ScaffoldColumn(false)]
        [JsonProperty(PropertyName = "lastUpdated")]
        public DateTime LastUpdated { get; set; }

        /// <summary>
        /// If this domain name matches a pre-authenticated guest requester, the guest will 
        /// automatically be enrolled and an invite sent out. Optionally, if there are any items
        /// in the Groups array, the new Guest user will be added to those Groups.
        /// </summary>
        [DisplayName("Domain name")]
        [Required(ErrorMessage = "Domain name (example.com) is required")]
        [JsonProperty(PropertyName = "domain")]
        public string DomainName { get; set; }

        /// <summary>
        /// String name of the email body template to use for pre-auth invitations handled by this record.
        /// Templates are stored in the /Templates folder
        /// </summary>
        [DisplayName("Invitation Template")]
        [JsonProperty(PropertyName = "invitationTemplate")]
        public string InvitationTemplate { get; set; }

        /// <summary>
        /// Optional link for guests approved via this tenant. After the email redemption link is clicked by the guest, and after
        /// the redemption is complete, the user's browser will be automatically redirected to this link.
        /// </summary>
        [DisplayName("Redirect URL")]
        [JsonProperty(PropertyName = "redirectLink")]
        public string RedirectLink { get; set; }

        /// <summary>
        /// Optional - Users matching the domain name will be automatically added to each group in this list
        /// </summary>
        [JsonProperty(PropertyName = "groups")]
        [DisplayName("Auto-Assigned Groups")]
        public List<string> Groups { get; set; }

        [DisplayName("Groups List")]
        [JsonIgnore]
        [ScaffoldColumn(false)]
        public string GroupsList { get; set; }

        public static async Task<IEnumerable<PreAuthDomain>> GetDomains()
        {
            return (await DocDBRepo.DB<PreAuthDomain>.GetItemsAsync()).OrderByDescending(c => c.LastUpdated);
        }

        public static async Task<PreAuthDomain> GetDomain(string id)
        {
            return (await DocDBRepo.DB<PreAuthDomain>.GetItemAsync(id));
        }

        public static async Task<PreAuthDomain> AddDomain(PreAuthDomain domain)
        {
            domain.CreateDate = DateTime.UtcNow;
            domain.LastUpdated = DateTime.UtcNow;
            
            return (await DocDBRepo.DB<PreAuthDomain>.CreateItemAsync(domain));
        }

        public static async Task<PreAuthDomain> UpdateDomain(PreAuthDomain domain)
        {
            domain.LastUpdated = DateTime.UtcNow;

            domain = (await DocDBRepo.DB<PreAuthDomain>.UpdateItemAsync(domain));

            return domain;
        }
        public static async Task<dynamic> DeleteDomain(PreAuthDomain domain)
        {
            return (await DocDBRepo.DB<PreAuthDomain>.DeleteItemAsync(domain));
        }
    }
}