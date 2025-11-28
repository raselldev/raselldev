namespace ErrorLogDashboard.Web.Models
{
    /// <summary>
    /// Dashboard summary statistics
    /// </summary>
    public class ErrorLogSummary
    {
        public int TotalErrors { get; set; }
        public int UnresolvedErrors { get; set; }
        public int ResolvedErrors { get; set; }
        public int UniqueErrorSources { get; set; }
        public int AffectedPlatforms { get; set; }
    }

    /// <summary>
    /// Platform distribution data for charts
    /// </summary>
    public class PlatformStats
    {
        public string Platform { get; set; }
        public int Count { get; set; }
    }

    /// <summary>
    /// App version distribution data for charts
    /// </summary>
    public class VersionStats
    {
        public string AppVersion { get; set; }
        public int Count { get; set; }
    }

    /// <summary>
    /// Error source distribution data for charts
    /// </summary>
    public class SourceStats
    {
        public string Source { get; set; }
        public int Count { get; set; }
    }

    /// <summary>
    /// Error trends over time data for charts
    /// </summary>
    public class TrendStats
    {
        public string Date { get; set; }
        public int Count { get; set; }
    }

    /// <summary>
    /// Resolution status statistics
    /// </summary>
    public class ResolutionStats
    {
        public int Resolved { get; set; }
        public int Unresolved { get; set; }
    }
}
