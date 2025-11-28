using System.Collections.Generic;
using System.Threading.Tasks;
using ErrorLogDashboard.Web.Models;

namespace ErrorLogDashboard.Web.Services
{
    /// <summary>
    /// Interface for error log service operations
    /// </summary>
    public interface IErrorLogService
    {
        // Read Operations
        Task<PagedResult<ErrorLog>> GetErrorLogsAsync(ErrorLogFilter filter);
        Task<ErrorLog> GetErrorLogByIdAsync(int id);
        Task<ErrorLogSummary> GetSummaryAsync();
        Task<IEnumerable<PlatformStats>> GetPlatformStatsAsync();
        Task<IEnumerable<VersionStats>> GetVersionStatsAsync();
        Task<IEnumerable<SourceStats>> GetTopSourcesAsync(int top = 10);
        Task<IEnumerable<TrendStats>> GetTrendsAsync(int days = 30);
        Task<ResolutionStats> GetResolutionStatsAsync();
        
        // Filter Options
        Task<IEnumerable<string>> GetDistinctPlatformsAsync();
        Task<IEnumerable<string>> GetDistinctVersionsAsync();
        Task<IEnumerable<string>> GetDistinctSourcesAsync();
        
        // Write Operations
        Task<bool> ResolveAsync(int id);
        Task<bool> UnresolveAsync(int id);
        Task<int> BulkResolveAsync(int[] ids);
        Task<int> BulkUnresolveAsync(int[] ids);
    }
}
