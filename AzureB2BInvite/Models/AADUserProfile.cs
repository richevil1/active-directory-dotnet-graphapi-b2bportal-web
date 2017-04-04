using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AzureB2BInvite.Models
{
    public class AADUserProfile
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "accountEnabled")]
        public bool AccountEnabled { get; set; }

        [JsonProperty(PropertyName = "businessPhones")]
        public string[] BusinessPhones { get; set; }

        [JsonProperty(PropertyName = "birthday")]
        public string Birthday { get; set; }

        [JsonProperty(PropertyName = "city")]
        public string City { get; set; }

        [JsonProperty(PropertyName = "country")]
        public string Country { get; set; }

        [JsonProperty(PropertyName = "department")]
        public string Department { get; set; }

        [DisplayName("Display Name")]
        [JsonProperty(PropertyName = "displayName")]
        public string DisplayName { get; set; }

        [DisplayName("Given Name")]
        [JsonProperty(PropertyName = "givenName")]
        public string GivenName { get; set; }

        [DisplayName("Job Title")]
        [JsonProperty(PropertyName = "jobTitle")]
        public string JobTitle { get; set; }

        [JsonProperty(PropertyName = "mail")]
        public string Mail { get; set; }

        [JsonProperty(PropertyName = "mailNickname")]
        public string MailNickname { get; set; }

        [DisplayName("Mobile Phone")]
        [JsonProperty(PropertyName = "mobilePhone")]
        public string MobilePhone { get; set; }

        [JsonProperty(PropertyName = "onPremisesDomainName")]
        public string OnPremisesDomainName { get; set; }

        [ScaffoldColumn(false)]
        [JsonProperty(PropertyName = "onPremisesImmutableId")]
        public string OnPremisesImmutableId { get; set; }

        [ScaffoldColumn(false)]
        [JsonProperty(PropertyName = "onPremisesLastSyncDateTime")]
        public string OnPremisesLastSyncDateTime { get; set; }

        [ScaffoldColumn(false)]
        [JsonProperty(PropertyName = "onPremisesSecurityIdentifier")]
        public string OnPremisesSecurityIdentifier { get; set; }

        [ScaffoldColumn(false)]
        [JsonProperty(PropertyName = "onPremisesSamAccountName")]
        public string OnPremisesSamAccountName { get; set; }

        [ScaffoldColumn(false)]
        [JsonProperty(PropertyName = "onPremisesSyncEnabled")]
        public string OnPremisesSyncEnabled { get; set; }

        [ScaffoldColumn(false)]
        [JsonProperty(PropertyName = "onPremisesUserPrincipalName")]
        public string OnPremisesUserPrincipalName { get; set; }

        [ScaffoldColumn(false)]
        [JsonProperty(PropertyName = "passwordPolicies")]
        public string PasswordPolicies { get; set; }

        [ScaffoldColumn(false)]
        [JsonProperty(PropertyName = "passwordProfile")]
        public dynamic PasswordProfile { get; set; }

        [DisplayName("Office Location")]
        [JsonProperty(PropertyName = "officeLocation")]
        public string OfficeLocation { get; set; }

        [DisplayName("Postal Code")]
        [JsonProperty(PropertyName = "postalCode")]
        public string PostalCode { get; set; }

        [DisplayName("Preferred Language")]
        [JsonProperty(PropertyName = "preferredLanguage")]
        public string PreferredLanguage { get; set; }

        [ScaffoldColumn(false)]
        [JsonProperty(PropertyName = "provisionedPlans")]
        public dynamic[] ProvisionedPlans { get; set; }

        [ScaffoldColumn(false)]
        [JsonProperty(PropertyName = "proxyAddresses")]
        public string[] ProxyAddresses { get; set; }

        [ScaffoldColumn(false)]
        [JsonProperty(PropertyName = "showInAddressList")]
        public string ShowInAddressList { get; set; }

        [ScaffoldColumn(false)]
        [JsonProperty(PropertyName = "imAddresses")]
        public string[] ImAddresses { get; set; }

        [JsonProperty(PropertyName = "state")]
        public string State { get; set; }

        [DisplayName("Street Address")]
        [JsonProperty(PropertyName = "streetAddress")]
        public string StreetAddress { get; set; }

        [JsonProperty(PropertyName = "surname")]
        public string Surname { get; set; }

        [ScaffoldColumn(false)]
        [JsonProperty(PropertyName = "usageLocation")]
        public string UsageLocation { get; set; }

        [JsonProperty(PropertyName = "userPrincipalName")]
        public string UserPrincipalName { get; set; }

        [ScaffoldColumn(false)]
        [JsonProperty(PropertyName = "userType")]
        public string UserType { get; set; }

        public static dynamic GetDeltaChanges(AADUserProfile orgUser, AADUserProfile newUser)
        {
            var res = new ExpandoObject();

            var props = typeof(AADUserProfile).GetProperties();
            foreach(var prop in props)
            {
                if (prop.Name == "AccountEnabled") continue;

                if (prop.GetValue(newUser) != null)
                {
                    if (!prop.GetValue(newUser).Equals(prop.GetValue(orgUser)))
                    {
                        AddProperty(res, prop.Name, prop.GetValue(newUser));
                    }
                }
            }
            return res;
        }
        private static void AddProperty(ExpandoObject expando, string propertyName, object propertyValue)
        {
            // ExpandoObject supports IDictionary so we can extend it like this
            var expandoDict = expando as IDictionary<string, object>;
            if (expandoDict.ContainsKey(propertyName))
                expandoDict[propertyName] = propertyValue;
            else
                expandoDict.Add(propertyName, propertyValue);
        }
    }
}
