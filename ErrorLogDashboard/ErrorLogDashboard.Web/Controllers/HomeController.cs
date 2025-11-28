using System.Web.Mvc;

namespace ErrorLogDashboard.Web.Controllers
{
    /// <summary>
    /// Home Controller for serving the dashboard view
    /// </summary>
    public class HomeController : Controller
    {
        /// <summary>
        /// Main dashboard page
        /// </summary>
        public ActionResult Index()
        {
            return View();
        }
    }
}
