using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using ErrorLogDashboard.Web.Models;

namespace ErrorLogDashboard.Web.Services
{
    /// <summary>
    /// Implementation of error log service using Dapper for database access
    /// </summary>
    public class ErrorLogService : IErrorLogService
    {
        private const string ConnectionStringName = "ErrorLogDb";
        private readonly string _connectionString;

        public ErrorLogService()
        {
            _connectionString = ConfigurationManager.ConnectionStrings[ConnectionStringName]?.ConnectionString 
                ?? throw new InvalidOperationException($"Connection string '{ConnectionStringName}' not found in configuration.");
        }

        public ErrorLogService(string connectionString)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }

        private IDbConnection CreateConnection()
        {
            return new SqlConnection(_connectionString);
        }

        #region Read Operations

        public async Task<PagedResult<ErrorLog>> GetErrorLogsAsync(ErrorLogFilter filter)
        {
            using (var connection = CreateConnection())
            {
                var whereClause = new StringBuilder("WHERE 1=1");
                var parameters = new DynamicParameters();

                // Apply filters
                if (!string.IsNullOrEmpty(filter.Platform))
                {
                    whereClause.Append(" AND Platform = @Platform");
                    parameters.Add("Platform", filter.Platform);
                }

                if (!string.IsNullOrEmpty(filter.AppVersion))
                {
                    whereClause.Append(" AND AppVersion = @AppVersion");
                    parameters.Add("AppVersion", filter.AppVersion);
                }

                if (!string.IsNullOrEmpty(filter.Source))
                {
                    whereClause.Append(" AND Source = @Source");
                    parameters.Add("Source", filter.Source);
                }

                if (filter.IsResolved.HasValue)
                {
                    whereClause.Append(" AND IsResolved = @IsResolved");
                    parameters.Add("IsResolved", filter.IsResolved.Value ? 1 : 0);
                }

                if (filter.StartDate.HasValue)
                {
                    whereClause.Append(" AND Timestamp >= @StartDate");
                    parameters.Add("StartDate", filter.StartDate.Value);
                }

                if (filter.EndDate.HasValue)
                {
                    whereClause.Append(" AND Timestamp <= @EndDate");
                    parameters.Add("EndDate", filter.EndDate.Value);
                }

                if (!string.IsNullOrEmpty(filter.Search))
                {
                    whereClause.Append(" AND (Message LIKE @Search OR StackTrace LIKE @Search OR Source LIKE @Search)");
                    parameters.Add("Search", "%" + filter.Search + "%");
                }

                // Validate sort column to prevent SQL injection
                var validSortColumns = new[] { "Id", "Timestamp", "Message", "Source", "AppVersion", "Platform", "DeviceInfo", "IsResolved" };
                var sortColumn = validSortColumns.Contains(filter.SortColumn, StringComparer.OrdinalIgnoreCase) 
                    ? filter.SortColumn 
                    : "Timestamp";
                var sortDirection = filter.SortDirection?.ToUpper() == "ASC" ? "ASC" : "DESC";

                // Count total records
                var countSql = $"SELECT COUNT(*) FROM HD_ERROR_LOG_V2 {whereClause}";
                var totalCount = await connection.ExecuteScalarAsync<int>(countSql, parameters);

                // Get paginated results
                var offset = (filter.Page - 1) * filter.PageSize;
                parameters.Add("Offset", offset);
                parameters.Add("PageSize", filter.PageSize);

                var sql = $@"
                    SELECT Id, Timestamp, Message, StackTrace, Source, AppVersion, Platform, DeviceInfo, IsResolved
                    FROM HD_ERROR_LOG_V2
                    {whereClause}
                    ORDER BY {sortColumn} {sortDirection}
                    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

                var items = await connection.QueryAsync<ErrorLog>(sql, parameters);

                return new PagedResult<ErrorLog>
                {
                    Items = items.ToArray(),
                    TotalCount = totalCount,
                    Page = filter.Page,
                    PageSize = filter.PageSize
                };
            }
        }

        public async Task<ErrorLog> GetErrorLogByIdAsync(int id)
        {
            using (var connection = CreateConnection())
            {
                var sql = @"
                    SELECT Id, Timestamp, Message, StackTrace, Source, AppVersion, Platform, DeviceInfo, IsResolved
                    FROM HD_ERROR_LOG_V2
                    WHERE Id = @Id";

                return await connection.QueryFirstOrDefaultAsync<ErrorLog>(sql, new { Id = id });
            }
        }

        public async Task<ErrorLogSummary> GetSummaryAsync()
        {
            using (var connection = CreateConnection())
            {
                var sql = @"
                    SELECT 
                        COUNT(*) AS TotalErrors,
                        SUM(CASE WHEN IsResolved = 0 THEN 1 ELSE 0 END) AS UnresolvedErrors,
                        SUM(CASE WHEN IsResolved = 1 THEN 1 ELSE 0 END) AS ResolvedErrors,
                        COUNT(DISTINCT Source) AS UniqueErrorSources,
                        COUNT(DISTINCT Platform) AS AffectedPlatforms
                    FROM HD_ERROR_LOG_V2";

                return await connection.QueryFirstOrDefaultAsync<ErrorLogSummary>(sql) ?? new ErrorLogSummary();
            }
        }

        public async Task<IEnumerable<PlatformStats>> GetPlatformStatsAsync()
        {
            using (var connection = CreateConnection())
            {
                var sql = @"
                    SELECT Platform, COUNT(*) AS Count
                    FROM HD_ERROR_LOG_V2
                    WHERE Platform IS NOT NULL AND Platform != ''
                    GROUP BY Platform
                    ORDER BY Count DESC";

                return await connection.QueryAsync<PlatformStats>(sql);
            }
        }

        public async Task<IEnumerable<VersionStats>> GetVersionStatsAsync()
        {
            using (var connection = CreateConnection())
            {
                var sql = @"
                    SELECT AppVersion, COUNT(*) AS Count
                    FROM HD_ERROR_LOG_V2
                    WHERE AppVersion IS NOT NULL AND AppVersion != ''
                    GROUP BY AppVersion
                    ORDER BY Count DESC";

                return await connection.QueryAsync<VersionStats>(sql);
            }
        }

        public async Task<IEnumerable<SourceStats>> GetTopSourcesAsync(int top = 10)
        {
            using (var connection = CreateConnection())
            {
                var sql = @"
                    SELECT TOP (@Top) Source, COUNT(*) AS Count
                    FROM HD_ERROR_LOG_V2
                    WHERE Source IS NOT NULL AND Source != ''
                    GROUP BY Source
                    ORDER BY Count DESC";

                return await connection.QueryAsync<SourceStats>(sql, new { Top = top });
            }
        }

        public async Task<IEnumerable<TrendStats>> GetTrendsAsync(int days = 30)
        {
            using (var connection = CreateConnection())
            {
                var sql = @"
                    SELECT CONVERT(VARCHAR(10), Timestamp, 120) AS Date, COUNT(*) AS Count
                    FROM HD_ERROR_LOG_V2
                    WHERE Timestamp >= DATEADD(DAY, -@Days, GETDATE())
                    GROUP BY CONVERT(VARCHAR(10), Timestamp, 120)
                    ORDER BY Date";

                return await connection.QueryAsync<TrendStats>(sql, new { Days = days });
            }
        }

        public async Task<ResolutionStats> GetResolutionStatsAsync()
        {
            using (var connection = CreateConnection())
            {
                var sql = @"
                    SELECT 
                        SUM(CASE WHEN IsResolved = 1 THEN 1 ELSE 0 END) AS Resolved,
                        SUM(CASE WHEN IsResolved = 0 THEN 1 ELSE 0 END) AS Unresolved
                    FROM HD_ERROR_LOG_V2";

                return await connection.QueryFirstOrDefaultAsync<ResolutionStats>(sql) ?? new ResolutionStats();
            }
        }

        #endregion

        #region Filter Options

        public async Task<IEnumerable<string>> GetDistinctPlatformsAsync()
        {
            using (var connection = CreateConnection())
            {
                var sql = @"
                    SELECT DISTINCT Platform
                    FROM HD_ERROR_LOG_V2
                    WHERE Platform IS NOT NULL AND Platform != ''
                    ORDER BY Platform";

                return await connection.QueryAsync<string>(sql);
            }
        }

        public async Task<IEnumerable<string>> GetDistinctVersionsAsync()
        {
            using (var connection = CreateConnection())
            {
                var sql = @"
                    SELECT DISTINCT AppVersion
                    FROM HD_ERROR_LOG_V2
                    WHERE AppVersion IS NOT NULL AND AppVersion != ''
                    ORDER BY AppVersion";

                return await connection.QueryAsync<string>(sql);
            }
        }

        public async Task<IEnumerable<string>> GetDistinctSourcesAsync()
        {
            using (var connection = CreateConnection())
            {
                var sql = @"
                    SELECT DISTINCT Source
                    FROM HD_ERROR_LOG_V2
                    WHERE Source IS NOT NULL AND Source != ''
                    ORDER BY Source";

                return await connection.QueryAsync<string>(sql);
            }
        }

        #endregion

        #region Write Operations

        public async Task<bool> ResolveAsync(int id)
        {
            using (var connection = CreateConnection())
            {
                var sql = "UPDATE HD_ERROR_LOG_V2 SET IsResolved = 1 WHERE Id = @Id";
                var affected = await connection.ExecuteAsync(sql, new { Id = id });
                return affected > 0;
            }
        }

        public async Task<bool> UnresolveAsync(int id)
        {
            using (var connection = CreateConnection())
            {
                var sql = "UPDATE HD_ERROR_LOG_V2 SET IsResolved = 0 WHERE Id = @Id";
                var affected = await connection.ExecuteAsync(sql, new { Id = id });
                return affected > 0;
            }
        }

        public async Task<int> BulkResolveAsync(int[] ids)
        {
            if (ids == null || ids.Length == 0)
                return 0;

            using (var connection = CreateConnection())
            {
                var sql = "UPDATE HD_ERROR_LOG_V2 SET IsResolved = 1 WHERE Id IN @Ids";
                return await connection.ExecuteAsync(sql, new { Ids = ids });
            }
        }

        public async Task<int> BulkUnresolveAsync(int[] ids)
        {
            if (ids == null || ids.Length == 0)
                return 0;

            using (var connection = CreateConnection())
            {
                var sql = "UPDATE HD_ERROR_LOG_V2 SET IsResolved = 0 WHERE Id IN @Ids";
                return await connection.ExecuteAsync(sql, new { Ids = ids });
            }
        }

        #endregion
    }
}
