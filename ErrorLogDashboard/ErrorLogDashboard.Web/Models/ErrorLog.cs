using System;

namespace ErrorLogDashboard.Web.Models
{
    /// <summary>
    /// Represents an error log entry from HD_ERROR_LOG_V2 table
    /// </summary>
    public class ErrorLog
    {
        public int Id { get; set; }
        public DateTime Timestamp { get; set; }
        public string Message { get; set; }
        public string StackTrace { get; set; }
        public string Source { get; set; }
        public string AppVersion { get; set; }
        public string Platform { get; set; }
        public string DeviceInfo { get; set; }
        public bool IsResolved { get; set; }

        // Computed property for display
        public string FormattedTimestamp => Timestamp.ToString("yyyy-MM-dd HH:mm:ss");
        public string StatusDisplay => IsResolved ? "Resolved" : "Unresolved";
        public string StatusClass => IsResolved ? "success" : "danger";
    }

    /// <summary>
    /// Represents an error log entry from VIEW_ERROR_LOG view (aggregated)
    /// </summary>
    public class ErrorLogView
    {
        public string Message { get; set; }
        public string StackTrace { get; set; }
        public string Source { get; set; }
        public string AppVersion { get; set; }
        public string Platform { get; set; }
        public string DeviceInfo { get; set; }
        public int TotalError { get; set; }
    }

    /// <summary>
    /// Request model for bulk operations
    /// </summary>
    public class BulkOperationRequest
    {
        public int[] Ids { get; set; }
    }

    /// <summary>
    /// Paginated result wrapper
    /// </summary>
    /// <typeparam name="T">Type of items</typeparam>
    public class PagedResult<T>
    {
        public T[] Items { get; set; }
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public bool HasPreviousPage => Page > 1;
        public bool HasNextPage => Page < TotalPages;
    }
}
