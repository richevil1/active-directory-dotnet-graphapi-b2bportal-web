using AzureB2BInvite.Models;
using Microsoft.Graph;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace AzureB2BInvite
{
    public static class AdalUtil
    {
        public static class Settings
        {
            public static string GraphResource = "https://graph.microsoft.com";
            public static string AADInstanceLocal { get; set; }
            public static string TenantID { get; set; }
            public static string Tenant { get; set; }
            public static string AADInstanceMulti { get; set; }
            public static string GraphApiVersion { get; set; }
            public static string AppClientId_Admin { get; set; }
            public static string WebAppUrl { get; set; }
            public static string AppClientSecret_Admin { get; set; }
            public static string AppClientId_Preauth { get; set; }
            public static string AppClientSecret_Preauth { get; set; }
            public static string InvitingOrganization { get; set; }
            public static RedemptionSettings SiteRedemptionSettings { get; set; }
            public static string InvitationEmailSubject { get; set; }
            public static string DefaultBodyTemplateName { get; set; }
            public static string[] InviterRoleNames { get; set; }
            public static string AssignedInviterRole { get; set; }
            public static bool UseSMTP { get; set; }
        }

        public static async Task<AuthenticationResult> AuthenticateApp(string graphResource=null)
        {
            string resource = (graphResource != null) ? graphResource : Settings.GraphResource;
            AuthenticationContext authContext = new AuthenticationContext(string.Format(Settings.AADInstanceLocal, Settings.TenantID));
            AuthenticationResult authResult = await authContext.AcquireTokenAsync(resource, new ClientCredential(Settings.AppClientId_Admin, Settings.AppClientSecret_Admin));
            return authResult;
        }

        public static async Task<AuthenticationResult> AuthenticateUser(string graphResource = null)
        {
            string resource = (graphResource != null) ? graphResource : Settings.GraphResource;

            AuthenticationContext authContext = new AuthenticationContext(string.Format(Settings.AADInstanceLocal, Settings.TenantID));
            AuthenticationResult authResult = await authContext.AcquireTokenSilentAsync(resource, Settings.AppClientId_Admin);
            return authResult;
        }

        public static AdalResponse CallGraph(string uri, dynamic postContent = null, bool isUpdate = false, string graphResource = null)
        {
            string resource = (graphResource != null) ? graphResource : Settings.GraphResource;

            var res = new AdalResponse
            {
                Successful = true
            };
            HttpResponseMessage response = null;
            try
            {
                AuthenticationResult authResult = null;

                // Get auth token
                var task = Task.Run(async () => {
                    authResult = await AuthenticateApp(resource);
                });
                task.Wait();

                string accessToken = authResult.AccessToken;

                var bearer = new AuthenticationHeaderValue("Bearer", accessToken);

                //getting inconsistent results (chunked vs not chunked) with async calls - switching to webclient/sync
                if (postContent != null)
                {
                    using (var httpClient = new HttpClient())
                    {
                        httpClient.Timeout = TimeSpan.FromSeconds(300);
                        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                        httpClient.DefaultRequestHeaders.Add("client-request-id", Guid.NewGuid().ToString());
                        httpClient.DefaultRequestHeaders.GetValues("client-request-id").Single();
                        string serialized = JsonConvert.SerializeObject(postContent);

                        HttpContent content = new StringContent(serialized, Encoding.UTF8, "application/json");

                        if (isUpdate)
                        {
                            var method = new HttpMethod("PATCH");
                            var request = new HttpRequestMessage(method, uri)
                            {
                                Content = content
                            };

                            response = httpClient.SendAsync(request).Result;
                        }
                        else
                        {
                            response = httpClient.PostAsync(uri, content).Result;
                        }
                        res.ResponseContent = response.Content.ReadAsStringAsync().Result;
                        res.StatusCode = response.StatusCode;
                        res.Message = response.ReasonPhrase;
                        response.EnsureSuccessStatusCode();
                    }
                }
                else
                {
                    using (var webClient = new WebClient())
                    {
                        webClient.Headers.Add("Authorization", bearer.ToString());
                        webClient.Headers.Add("client-request-id", Guid.NewGuid().ToString());

                        res.ResponseContent = webClient.DownloadString(uri);
                    }
                }
                return res;
            }
            catch (WebException ex)
            {
                res.Successful = false;
                using (var reader = new StreamReader(ex.Response.GetResponseStream()))
                {
                    res.ResponseContent = reader.ReadToEnd();
                }

                var serverError = JsonConvert.DeserializeObject<GraphError>(res.ResponseContent);
                var reason = (response == null ? "N/A" : response.ReasonPhrase);
                var serverErrorMessage = (serverError.Error == null) ? "N/A" : serverError.Error.Message;
                res.Message = string.Format("{0} (Server response: {1}. Server detail: {2})", ex.Message, reason, serverErrorMessage);
                return res;
            }
        }

        public static OIDConfigResponse FindPublicAADTenant(string domainName)
        {
            var uri = string.Format("https://login.windows.net/{0}/.well-known/openid-configuration", domainName);
            string res = "";
            using (var web = new WebClient())
            {
                try
                {
                    res = web.DownloadString(uri);
                }
                catch (WebException exception)
                {
                    using (var reader = new StreamReader(exception.Response.GetResponseStream()))
                    {
                        res = reader.ReadToEnd();
                    }
                }
                return JsonConvert.DeserializeObject<OIDConfigResponse>(res);
            }
        }
    }
    public class OIDConfigResponse
    {
        [JsonProperty(PropertyName = "authorization_endpoint")]
        public string AuthorizationEndpoint { get; set; }

        [JsonProperty(PropertyName = "token_endpoint")]
        public string TokenEndpoint { get; set; }

        [JsonProperty(PropertyName = "token_endpoint_auth_methods_supported")]
        public string[] TokenEndpointAuthMethodsSupported { get; set; }

        [JsonProperty(PropertyName = "jwks_uri")]
        public string JwksUri { get; set; }

        [JsonProperty(PropertyName = "response_modes_supported")]
        public string[] ResponseModesSupported { get; set; }

        [JsonProperty(PropertyName = "subject_types_supported")]
        public string[] SubjectTypesSupported { get; set; }

        [JsonProperty(PropertyName = "id_token_signing_alg_values_supported")]
        public string[] IdTokenSigningAlgValuesSupported { get; set; }

        [JsonProperty(PropertyName = "http_logout_supported")]
        public bool HttpLogoutSupported { get; set; }

        [JsonProperty(PropertyName = "frontchannel_logout_supported")]
        public bool FrontchannelLogoutSupported { get; set; }

        [JsonProperty(PropertyName = "end_session_endpoint")]
        public string EndSessionEndpoint { get; set; }

        [JsonProperty(PropertyName = "response_types_supported")]
        public string[] ResponseTypesSupported { get; set; }

        [JsonProperty(PropertyName = "scopes_supported")]
        public string[] ScopesSupported { get; set; }

        [JsonProperty(PropertyName = "issuer")]
        public string Issuer { get; set; }

        [JsonProperty(PropertyName = "claims_supported")]
        public string[] ClaimsSupported { get; set; }

        [JsonProperty(PropertyName = "microsoft_multi_refresh_token")]
        public string MicrosoftMultiRefreshToken { get; set; }

        [JsonProperty(PropertyName = "check_session_iframe")]
        public string CheckSessionIframe { get; set; }

        [JsonProperty(PropertyName = "userinfo_endpoint")]
        public string UserInfoEndpoint { get; set; }

        [JsonProperty(PropertyName = "tenant_region_scope")]
        public string TenantRegionScope { get; set; }

        [JsonProperty(PropertyName = "cloud_instance_name")]
        public string CloudInstanceName { get; set; }

        [JsonProperty(PropertyName = "error")]
        public string Error { get; set; }
    }

    public class OIDConfigError
    {
        [JsonProperty(PropertyName = "error")]
        public string Error { get; set; }

        [JsonProperty(PropertyName = "error_description")]
        public string ErrorDescription { get; set; }

        [JsonProperty(PropertyName = "error_codes")]
        public string[] ErrorCodes { get; set; }

        [JsonProperty(PropertyName = "Timestamp")]
        public DateTime Timestamp { get; set; }

        [JsonProperty(PropertyName = "trace_id")]
        public string TraceID { get; set; }

        [JsonProperty(PropertyName = "correlation_id")]
        public string CorrelationId { get; set; }
    }

    public class AdalResponse
    {
        public string ResponseContent { get; set; }
        public HttpStatusCode StatusCode { get; set; }
        public bool Successful { get; set; }
        public string Message { get; set; }
    }
}