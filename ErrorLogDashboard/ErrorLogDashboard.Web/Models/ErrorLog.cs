using System;
using System.Collections.Generic;

namespace ErrorLogDashboard.Web.Models
{
    /// <summary>
    /// Represents an error log entry from VIEW_ERROR_LOG
    /// </summary>
    public class ErrorLog
    {
        public int Id { get; set; }
        public string Message { get; set; }
        public string StackTrace { get; set; }
        public string Source { get; set; }
        public string AppVersion { get; set; }
        public string Platform { get; set; }
        public string DeviceInfo { get; set; }
        public int TotalError { get; set; }
    }

    /// <summary>
    /// Dashboard summary statistics
    /// </summary>
    public class DashboardSummary
    {
        public int TotalErrors { get; set; }
        public int UniqueErrorSources { get; set; }
        public int AffectedPlatforms { get; set; }
        public string MostAffectedAppVersion { get; set; }
    }

    /// <summary>
    /// Grouped count result for charts
    /// </summary>
    public class GroupedCount
    {
        public string Name { get; set; }
        public int Count { get; set; }
    }

    /// <summary>
    /// Paginated result wrapper
    /// </summary>
    public class PagedResult<T>
    {
        public List<T> Data { get; set; }
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }

    /// <summary>
    /// Filter parameters for error log queries
    /// </summary>
    public class ErrorLogFilter
    {
        public string Platform { get; set; }
        public string AppVersion { get; set; }
        public string Source { get; set; }
        public string SearchTerm { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string SortBy { get; set; } = "TotalError";
        public bool SortDescending { get; set; } = true;
    }
}
