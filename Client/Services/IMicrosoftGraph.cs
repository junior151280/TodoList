using System.Threading.Tasks;
using Microsoft.Graph;

namespace TodoListClient.Services
{
    public interface IMicrosoftGraph
    {
        Task<string> GetString();
        Task<Site> GetSiteByName(string siteName);
        Task<User> GetUserInfo();
        Task<List> GetLisByName(string ListName);
        Task<Site> GetSiteBySiteID(string siteId);
        Task SetList(string siteId);
        Task SetItemList(string siteId, string listid);
    }
}
