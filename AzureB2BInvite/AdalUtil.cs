using AzureB2BInvite.Models;
using Microsoft.Graph;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Newtonsoft.Json;
using System;
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
            public static string AADInstanceMulti { get; set; }
            public static string GraphApiVersion { get; set; }
            public static string AppClientId_Admin { get; set; }
            public static string AppClientSecret_Admin { get; set; }
            public static string InvitingOrganization { get; set; }
            public static RedemptionSettings SiteRedemptionSettings { get; set; }
            public static string InvitationEmailSubject { get; set; }
            public static string DefaultBodyTemplateName { get; set; }
            public static string[] InviterRoleNames { get; set; }
            public static string AssignedInviterRole { get; set; }
            public static bool UseSMTP { get; set; }
        }

        public static async Task<AuthenticationResult> Authenticate()
        {
            AuthenticationContext authContext = new AuthenticationContext(string.Format(Settings.AADInstanceLocal, Settings.TenantID));
            AuthenticationResult authResult = await authContext.AcquireTokenAsync(Settings.GraphResource, new ClientCredential(Settings.AppClientId_Admin, Settings.AppClientSecret_Admin));
            return authResult;
        }

        public static AdalResponse CallGraph(string uri, dynamic postContent = null, bool isUpdate = false)
        {
            var res = new AdalResponse
            {
                Successful = true
            };
            HttpResponseMessage response = null;
            try
            {
                // Get auth token
                AuthenticationResult authResult = Authenticate().Result;

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
            catch (Exception ex)
            {
                res.Successful = false;
                var serverError = JsonConvert.DeserializeObject<GraphError>(res.ResponseContent);
                
                var reason = (response == null ? "N/A" : response.ReasonPhrase);
                var serverErrorMessage = (serverError.Error ==null) ? "N/A" : serverError.Error.Message;
                res.Message = string.Format("{0} (Server response: {1}. Server detail: {2})", ex.Message, reason, serverErrorMessage);
                return res;
            }
        }
    }

    public class AdalResponse
    {
        public string ResponseContent { get; set; }
        public HttpStatusCode StatusCode { get; set; }
        public bool Successful { get; set; }
        public string Message { get; set; }
    }
}