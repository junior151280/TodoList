using Azure.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Graph;
using Microsoft.Identity.Web;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace TodoListClient.Services
{
    public static class MicrosoftGraphServiceExtensions
    {
        public static void AddMicrosoftGraphService(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddHttpClient<IMicrosoftGraph, MicrosoftGraphService>();
        }
    }
    public class MicrosoftGraphService : IMicrosoftGraph
    {
        private GraphServiceClient _graphService;
        private readonly string _tenantId = string.Empty;
        private readonly string _clientId = string.Empty;
        private readonly string _clientSecret = string.Empty;
        private readonly string _microsoftGraph = string.Empty;
        private readonly string _redirectUri = string.Empty;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly HttpClient _httpClient;
        private readonly ITokenAcquisition _tokenAcquisition;
        //private readonly string siteId = "m365x03627074.sharepoint.com,3db22675-a0d6-4217-853c-f0cb280367aa,81a80726-ff0c-484f-975d-f5c58dce817e";
        private readonly string siteId = "PocSefaz,4f19c27e-4f42-49e1-bd5c-40423a13b9f3";

        /// <summary>
        /// MicrosoftGraphService
        /// </summary>
        /// <param name="tokenAcquisition"></param>
        /// <param name="httpClient"></param>
        /// <param name="configuration"></param>
        /// <param name="contextAccessor"></param>
        public MicrosoftGraphService(ITokenAcquisition tokenAcquisition, HttpClient httpClient, IConfiguration configuration, IHttpContextAccessor contextAccessor)
        {
            _clientId = configuration["AzureAd:ClientId"];
            _tenantId = configuration["AzureAd:TenantId"];
            _clientSecret = configuration["AzureAd:ClientSecret"];
            _microsoftGraph = configuration["AzureAd:MicrosoftGraph"];
            _redirectUri = configuration["AzureAd:Callback"];
            _httpClient = httpClient;
            _tokenAcquisition = tokenAcquisition;
            _contextAccessor = contextAccessor;
        }

        /// <summary>
        /// PrepareAuthenticatedClientSecretCredential
        /// </summary>
        /// <returns></returns>
        private void PrepareAuthenticatedClientSecretCredential()
        {
            var scopes = new[] { _microsoftGraph };
            var options = new TokenCredentialOptions
            {
                AuthorityHost = AzureAuthorityHosts.AzurePublicCloud
            };

            // https://docs.microsoft.com/dotnet/api/azure.identity.clientsecretcredential
            var clientSecretCredential = new ClientSecretCredential(
                _tenantId, _clientId, _clientSecret, options);

            _graphService = new GraphServiceClient(clientSecretCredential, scopes);
        }

        /// <summary>
        /// PrepareAuthenticatedAuthProvider
        /// </summary>
        private void PrepareAuthenticatedAuthProvider()
        {
            var scopes = new[] { _microsoftGraph };
            var accessToken2 = _tokenAcquisition.GetAccessTokenForUserAsync(scopes).Result;
            var authProvider = new DelegateAuthenticationProvider(async (request) =>
            {
                request.Headers.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken2);
            });
            _graphService = new GraphServiceClient(authProvider);
        }

        /// <summary>
        /// GetString
        /// </summary>
        /// <returns></returns>
        public Task<string> GetString()
        {
            using HttpClient client = new();
            var uri = $"https://login.microsoftonline.com/{_tenantId}/oauth2/v2.0/authorize?client_id={_clientId}&response_type=code&redirect_uri={_redirectUri}&response_mode=query&scope={_microsoftGraph}";

            var response = client.GetAsync(uri).Result;

            if (response.IsSuccessStatusCode)
            {
                return response.Content.ReadAsStringAsync();
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// GetLisByName
        /// </summary>
        /// <param name="ListName"></param>
        /// <returns></returns>
        public async Task<List> GetLisByName(string ListName)
        {
            PrepareAuthenticatedClientSecretCredential();

            var items = await _graphService.Sites[siteId].Lists
                .Request()
                .GetAsync();

            List list = new();

            foreach (var item in items)
            {
                if (item.DisplayName == ListName)
                {
                    list = item;
                }
            }
            return list;
        }

        /// <summary>
        /// GetUserInfo
        /// </summary>
        /// <returns></returns>
        public async Task<User> GetUserInfo()
        {
            PrepareAuthenticatedClientSecretCredential();
            User userAzure = await _graphService.Me
                .Request()
                .GetAsync();
            return userAzure;
        }

        /// <summary>
        /// GetSiteByName
        /// </summary>
        /// <param name="siteName"></param>
        /// <returns></returns>
        public async Task<Site> GetSiteByName(string siteName)
        {
            //var response = await GetString();
            PrepareAuthenticatedAuthProvider();
            try
            {
                Site site = new();
                var items = await _graphService.Sites
                    .Request()
                    .GetAsync();

                var nameSite = items.Where(item => item.DisplayName == siteName);

                var displayName = new List<string>();

                foreach (var item in items)
                {
                    displayName.Add(item.DisplayName);

                    if (item.DisplayName == "siteName")
                    {
                        site = item;
                    }
                }
                return site;

            }
            catch (System.Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// GetSiteBySiteID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<Site> GetSiteBySiteID(string id)
        {
            PrepareAuthenticatedAuthProvider();
            try
            {
                var site = await _graphService.Sites[id]
                    .Request()
                    .GetAsync();

                return site;
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// SetList
        /// </summary>
        public async Task SetList(string id)
        {
            try
            {
                PrepareAuthenticatedAuthProvider();
                var list = new List
                {
                    DisplayName = "Books4",
                    Columns = new ListColumnsCollectionPage()
                    {
                        new ColumnDefinition
                        {
                            Name = "Autor",
                            Text = new TextColumn
                            {
                            }
                        },
                        new ColumnDefinition
                        {
                            Name = "PageCount",
                            Number = new NumberColumn
                            {
                            }
                        }
                    },
                    ListInfo = new ListInfo
                    {
                        Template = "genericList"
                    }
                };

                var reuslt = await _graphService.Sites[id].Lists
                    .Request()
                    .AddAsync(list);

                var listItem = new ListItem
                {
                    Fields = new FieldValueSet
                    {
                        AdditionalData = new Dictionary<string, object>()
                        {
                            {"Title", "Meu Book"},
                            {"Autor", "Jailton Jr"},
                            {"PageCount", 100}
                        }
                    }
                };

                var reuslt2 = await _graphService.Sites[id].Lists[reuslt.Id].Items
                    .Request()
                    .AddAsync(listItem);

                await SetItemList(id, reuslt2.Id);

            }
            catch (System.Exception)
            {

                throw;
            }
        }

        /// <summary>
        /// SetItemList
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="listId"></param>
        /// <returns></returns>
        public async Task SetItemList(string siteId, string listId)
        {
            try
            {
                PrepareAuthenticatedAuthProvider();
                var listItem = new ListItem
                {
                    Fields = new FieldValueSet
                    {
                        AdditionalData = new Dictionary<string, object>()
                        {
                            {"Title", "Meu Book"},
                            {"Autor", "Jailton Jr"},
                            {"PageCount", 100}
                        }
                    }
                };

                var reuslt2 = await _graphService.Sites[siteId].Lists[listId].Items
                    .Request()
                    .AddAsync(listItem);
            }
            catch (System.Exception)
            {
                throw;
            }
        }
    }
}