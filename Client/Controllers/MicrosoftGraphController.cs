using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using TodoListClient.Services;


namespace TodoListClient.Controllers
{
    public class MicrosoftGraphController : Controller
    {
        private readonly IMicrosoftGraph _microsoftGraph;

        public MicrosoftGraphController(IMicrosoftGraph microsoftGraph)
        {
            _microsoftGraph = microsoftGraph;
        }
        public async Task<ActionResult> Index()
        {
            //Delegated 
            //var user = await _microsoftGraph.GetUserInfo();
            var itens = await _microsoftGraph.GetSiteBySiteID("<Hostname>.sharepoint.com:/sites/<SiteName>");
            await _microsoftGraph.SetList(itens.Id) ;

            //Application
            //var itens = await _microsoftGraph.GetSiteByName("<SiteName>");

            return View(itens);
        }
    }
}