using System;

namespace ErrorLogDashboard.Web.Models
{
    /// <summary>
    /// Filter parameters for querying error logs
    /// </summary>
    public class ErrorLogFilter
    {
        public string Platform { get; set; }
        public string AppVersion { get; set; }
        public string Source { get; set; }
        public bool? IsResolved { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Search { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string SortColumn { get; set; } = "Timestamp";
        public string SortDirection { get; set; } = "DESC";
    }
}
