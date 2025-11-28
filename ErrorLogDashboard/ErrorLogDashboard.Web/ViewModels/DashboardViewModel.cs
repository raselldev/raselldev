using System.Collections.Generic;
using ErrorLogDashboard.Web.Models;

namespace ErrorLogDashboard.Web.ViewModels
{
    /// <summary>
    /// View model for the dashboard page
    /// </summary>
    public class DashboardViewModel
    {
        public ErrorLogSummary Summary { get; set; }
        public IEnumerable<string> Platforms { get; set; }
        public IEnumerable<string> Versions { get; set; }
        public IEnumerable<string> Sources { get; set; }

        public DashboardViewModel()
        {
            Summary = new ErrorLogSummary();
            Platforms = new List<string>();
            Versions = new List<string>();
            Sources = new List<string>();
        }
    }
}
