using System.Threading.Tasks;
using System.Web.Mvc;
using ErrorLogDashboard.Web.Services;
using ErrorLogDashboard.Web.ViewModels;

namespace ErrorLogDashboard.Web.Controllers
{
    /// <summary>
    /// MVC controller for dashboard views
    /// </summary>
    public class HomeController : Controller
    {
        private readonly IErrorLogService _errorLogService;

        public HomeController()
        {
            _errorLogService = new ErrorLogService();
        }

        public HomeController(IErrorLogService errorLogService)
        {
            _errorLogService = errorLogService;
        }

        /// <summary>
        /// Main dashboard view
        /// </summary>
        public async Task<ActionResult> Index()
        {
            var viewModel = new DashboardViewModel
            {
                Summary = await _errorLogService.GetSummaryAsync(),
                Platforms = await _errorLogService.GetDistinctPlatformsAsync(),
                Versions = await _errorLogService.GetDistinctVersionsAsync(),
                Sources = await _errorLogService.GetDistinctSourcesAsync()
            };

            return View(viewModel);
        }
    }
}
