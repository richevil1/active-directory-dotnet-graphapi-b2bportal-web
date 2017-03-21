using B2BPortal.Infrastructure;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;

namespace B2BPortal.B2B
{
    public static class AdalUtil
    {
        public static async Task<AuthenticationResult> Authenticate()
        {
            AuthenticationContext authContext = new AuthenticationContext(string.Format(Settings.AADInstanceLocal, Settings.TenantID));
            try
            {
                AuthenticationResult authResult = await authContext.AcquireTokenAsync(Settings.GraphResource, new ClientCredential(Settings.AppClientId_Admin, Settings.AppClientSecret_Admin));
                return authResult;
            }
            catch (AdalException ex)
            {
                Logging.WriteToAppLog("Error occured during app authentication", System.Diagnostics.EventLogEntryType.Error, ex);
                return null;
            }
        }

        public static async Task<string> CallGraph(string uri, dynamic postContent=null)
        {
            // Get auth token
            AuthenticationResult authResult = await Authenticate();
            string accessToken = authResult.AccessToken;
            string serverResponse = "";

            try
            {
                using (var httpClient = new HttpClient())
                {
                    httpClient.Timeout = TimeSpan.FromSeconds(300);
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                    httpClient.DefaultRequestHeaders.Add("client-request-id", Guid.NewGuid().ToString());
                    httpClient.DefaultRequestHeaders.GetValues("client-request-id").Single();

                    HttpResponseMessage postResponse;

                    if (postContent != null)
                    {
                        HttpContent content = new StringContent(JsonConvert.SerializeObject(postContent));
                        content.Headers.Add("ContentType", "application/json");
                        postResponse = await httpClient.PostAsync(uri, content);
                    }
                    else
                    {
                        postResponse = httpClient.GetAsync(uri).Result;
                    }

                    serverResponse = await postResponse.Content.ReadAsStringAsync();
                    return serverResponse;
                }
            }
            catch (Exception ex)
            {
                Logging.WriteToAppLog("Error calling Graph.", System.Diagnostics.EventLogEntryType.Error, ex);
                throw;
            }
        }
    }
}