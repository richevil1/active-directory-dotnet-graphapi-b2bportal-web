using Microsoft.Graph;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace AzureB2BInvite
{
    public class GraphUtil
    {
        private GraphServiceClient _client;

        public GraphUtil()
        {
            AuthenticationResult authResult = AdalUtil.AuthenticateApp().Result;
            string accessToken = authResult.AccessToken;

            _client = new GraphServiceClient(
                new DelegateAuthenticationProvider(
                    (requestMessage) =>
                    {
                        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("bearer", accessToken);
                        return Task.FromResult(0);
                    }));
        }

        public async Task<User> GetUser(string upn)
        {
           return await (_client.Users[upn]).Request().GetAsync();
        }
        public async Task<IEnumerable<Group>> GetGroups()
        {
            return await (_client.Groups).Request().GetAsync();
        }

        public async Task<User> SetUser(User user)
        {
            try
            {
                var u = _client.Users[user.UserPrincipalName].Request();

                user = await u.UpdateAsync(user);

                return user;
            }
            catch (Exception ex)
            {
                throw new Exception("Problem updating user", ex);
            }
        }
    }
}
