using AzureB2BInvite;
using B2BPortal.Data;
using B2BPortal.Infrastructure;
using B2BPortal.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Westwind.Web.Utilities;

namespace B2BPortal.Models
{
    public class InviteTemplate : DocModelBase, IDocModelBase
    {

        /// <summary>
        /// Site name displayed on home page (above description)
        /// </summary>
        [DisplayName("Template Name")]
        [Required]
        [JsonProperty(PropertyName = "templateName")]
        public string TemplateName { get; set; }

        /// <summary>
        /// Name of inviting organization
        /// </summary>
        [DisplayName("Template Content")]
        [AllowHtml]
        [Required]
        [JsonProperty(PropertyName = "templateContent")]
        public string TemplateContent { get; set; }

        /// <summary>
        /// Welcome message displayed on the home page - HTML allowed
        /// </summary>
        [DisplayName("Subject Template")]
        [Required]
        [JsonProperty(PropertyName = "subjectTemplate")]
        public string SubjectTemplate { get; set; }

        /// <summary>
        /// Date this version of settings was committed
        /// </summary>
        [ScaffoldColumn(false)]
        [DisplayName("Last Updated")]
        [JsonProperty(PropertyName = "lastUpdated")]
        public DateTime LastUpdated { get; set; }

        [ScaffoldColumn(false)]
        [DisplayName("Template Version")]
        [JsonProperty(PropertyName = "templateVersion")]
        public int TemplateVersion { get; set; }

        [DisplayName("Template Author")]
        [JsonProperty(PropertyName = "templateAuthor")]
        public string TemplateAuthor { get; set; }

        public static async Task<IEnumerable<InviteTemplate>> GetTemplates()
        {
            return (await DocDBRepo.DB<InviteTemplate>.GetItemsAsync()).OrderByDescending(c => c.LastUpdated);
        }

        public static async Task<InviteTemplate> GetTemplate(string id)
        {
            return (await DocDBRepo.DB<InviteTemplate>.GetItemAsync(id));
        }

        public static async Task<InviteTemplate> AddTemplate(InviteTemplate template)
        {
            template.LastUpdated = DateTime.UtcNow;
            return (await DocDBRepo.DB<InviteTemplate>.CreateItemAsync(template));
        }

        public static async Task<IEnumerable<InviteTemplate>> InitializeDefaultTemplate(string templateAuthor)
        {
            var template = new InviteTemplate
            {
                LastUpdated = DateTime.UtcNow,
                TemplateAuthor = templateAuthor,
                TemplateName = "Default",
                TemplateVersion = 1
            };

            template.SubjectTemplate = AdalUtil.Settings.InvitationEmailSubject;
            template.TemplateContent = Settings.GetMailTemplate(AdalUtil.Settings.DefaultBodyTemplateName);

            template = (await DocDBRepo.DB<InviteTemplate>.CreateItemAsync(template));
            var res = new List<InviteTemplate>();
            res.Add(template);
            return res;
        }

        public static async Task<InviteTemplate> UpdateTemplate(InviteTemplate template)
        {
            template.LastUpdated = DateTime.UtcNow;
            template.TemplateVersion++;

            //TOSDocument is decorated with [AllowHtml], so clearing out dangerous tags
            template.TemplateContent = HtmlSanitizer.SanitizeHtml(template.TemplateContent);

            template = (await DocDBRepo.DB<InviteTemplate>.UpdateItemAsync(template));

            return template;
        }
    }
}