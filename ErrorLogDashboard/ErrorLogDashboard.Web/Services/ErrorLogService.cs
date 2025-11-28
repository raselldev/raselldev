using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using ErrorLogDashboard.Web.Models;

namespace ErrorLogDashboard.Web.Services
{
    /// <summary>
    /// Service for accessing error log data from the database
    /// </summary>
    public class ErrorLogService
    {
        private readonly string _connectionString;

        public ErrorLogService()
        {
            _connectionString = ConfigurationManager.ConnectionStrings["ErrorLogDb"]?.ConnectionString
                ?? throw new ConfigurationErrorsException("Connection string 'ErrorLogDb' not found in configuration.");
        }

        /// <summary>
        /// Gets paginated and filtered error logs
        /// </summary>
        public PagedResult<ErrorLog> GetErrorLogs(ErrorLogFilter filter)
        {
            var errors = new List<ErrorLog>();
            int totalCount = 0;

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                // Build the base query with filtering
                var whereClause = BuildWhereClause(filter);
                
                // Get total count
                using (var countCmd = new SqlCommand($"SELECT COUNT(*) FROM VIEW_ERROR_LOG {whereClause}", connection))
                {
                    AddFilterParameters(countCmd, filter);
                    totalCount = (int)countCmd.ExecuteScalar();
                }

                // Build order by clause
                var orderBy = GetOrderByClause(filter);
                var offset = (filter.Page - 1) * filter.PageSize;

                // Get paginated data
                var query = $@"
                    SELECT message, StackTrace, source, AppVersion, Platform, DeviceInfo, TOTAL_ERROR,
                           ROW_NUMBER() OVER ({orderBy}) as RowNum
                    FROM VIEW_ERROR_LOG
                    {whereClause}
                    {orderBy}
                    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

                using (var cmd = new SqlCommand(query, connection))
                {
                    AddFilterParameters(cmd, filter);
                    cmd.Parameters.AddWithValue("@Offset", offset);
                    cmd.Parameters.AddWithValue("@PageSize", filter.PageSize);

                    using (var reader = cmd.ExecuteReader())
                    {
                        int rowId = offset + 1;
                        while (reader.Read())
                        {
                            errors.Add(MapErrorLog(reader, rowId++));
                        }
                    }
                }
            }

            return new PagedResult<ErrorLog>
            {
                Data = errors,
                TotalCount = totalCount,
                Page = filter.Page,
                PageSize = filter.PageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / filter.PageSize)
            };
        }

        /// <summary>
        /// Gets dashboard summary statistics
        /// </summary>
        public DashboardSummary GetSummary()
        {
            var summary = new DashboardSummary();

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                var query = @"
                    SELECT 
                        ISNULL(SUM(TOTAL_ERROR), 0) as TotalErrors,
                        COUNT(DISTINCT source) as UniqueErrorSources,
                        COUNT(DISTINCT Platform) as AffectedPlatforms
                    FROM VIEW_ERROR_LOG";

                using (var cmd = new SqlCommand(query, connection))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            summary.TotalErrors = reader.GetInt32(0);
                            summary.UniqueErrorSources = reader.GetInt32(1);
                            summary.AffectedPlatforms = reader.GetInt32(2);
                        }
                    }
                }

                // Get most affected app version
                var versionQuery = @"
                    SELECT TOP 1 AppVersion
                    FROM VIEW_ERROR_LOG
                    GROUP BY AppVersion
                    ORDER BY SUM(TOTAL_ERROR) DESC";

                using (var cmd = new SqlCommand(versionQuery, connection))
                {
                    var result = cmd.ExecuteScalar();
                    summary.MostAffectedAppVersion = result?.ToString() ?? "N/A";
                }
            }

            return summary;
        }

        /// <summary>
        /// Gets error count grouped by platform
        /// </summary>
        public List<GroupedCount> GetPlatformStats()
        {
            return GetGroupedStats("Platform");
        }

        /// <summary>
        /// Gets error count grouped by app version
        /// </summary>
        public List<GroupedCount> GetVersionStats()
        {
            return GetGroupedStats("AppVersion");
        }

        /// <summary>
        /// Gets error count grouped by source (top 10)
        /// </summary>
        public List<GroupedCount> GetSourceStats()
        {
            var stats = new List<GroupedCount>();

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                var query = @"
                    SELECT TOP 10 source as Name, SUM(TOTAL_ERROR) as Count
                    FROM VIEW_ERROR_LOG
                    WHERE source IS NOT NULL AND source <> ''
                    GROUP BY source
                    ORDER BY Count DESC";

                using (var cmd = new SqlCommand(query, connection))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            stats.Add(new GroupedCount
                            {
                                Name = reader.GetString(0),
                                Count = reader.GetInt32(1)
                            });
                        }
                    }
                }
            }

            return stats;
        }

        /// <summary>
        /// Gets a specific error log by its row number/id
        /// </summary>
        public ErrorLog GetErrorById(int id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                var query = @"
                    SELECT message, StackTrace, source, AppVersion, Platform, DeviceInfo, TOTAL_ERROR
                    FROM (
                        SELECT message, StackTrace, source, AppVersion, Platform, DeviceInfo, TOTAL_ERROR,
                               ROW_NUMBER() OVER (ORDER BY TOTAL_ERROR DESC, source) as RowNum
                        FROM VIEW_ERROR_LOG
                    ) AS Numbered
                    WHERE RowNum = @Id";

                using (var cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@Id", id);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return MapErrorLog(reader, id);
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Gets distinct values for filter dropdowns
        /// </summary>
        public List<string> GetDistinctValues(string columnName)
        {
            var values = new List<string>();
            var validColumns = new[] { "Platform", "AppVersion", "source" };

            if (!validColumns.Contains(columnName, StringComparer.OrdinalIgnoreCase))
            {
                throw new ArgumentException($"Invalid column name: {columnName}");
            }

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                var query = $@"
                    SELECT DISTINCT {columnName}
                    FROM VIEW_ERROR_LOG
                    WHERE {columnName} IS NOT NULL AND {columnName} <> ''
                    ORDER BY {columnName}";

                using (var cmd = new SqlCommand(query, connection))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            values.Add(reader.GetString(0));
                        }
                    }
                }
            }

            return values;
        }

        private List<GroupedCount> GetGroupedStats(string columnName)
        {
            var stats = new List<GroupedCount>();

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                var query = $@"
                    SELECT {columnName} as Name, SUM(TOTAL_ERROR) as Count
                    FROM VIEW_ERROR_LOG
                    WHERE {columnName} IS NOT NULL AND {columnName} <> ''
                    GROUP BY {columnName}
                    ORDER BY Count DESC";

                using (var cmd = new SqlCommand(query, connection))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            stats.Add(new GroupedCount
                            {
                                Name = reader.GetString(0),
                                Count = reader.GetInt32(1)
                            });
                        }
                    }
                }
            }

            return stats;
        }

        private string BuildWhereClause(ErrorLogFilter filter)
        {
            var conditions = new List<string>();

            if (!string.IsNullOrEmpty(filter.Platform))
            {
                conditions.Add("Platform = @Platform");
            }

            if (!string.IsNullOrEmpty(filter.AppVersion))
            {
                conditions.Add("AppVersion = @AppVersion");
            }

            if (!string.IsNullOrEmpty(filter.Source))
            {
                conditions.Add("source = @Source");
            }

            if (!string.IsNullOrEmpty(filter.SearchTerm))
            {
                conditions.Add("(message LIKE @SearchTerm OR StackTrace LIKE @SearchTerm OR source LIKE @SearchTerm)");
            }

            return conditions.Any() ? "WHERE " + string.Join(" AND ", conditions) : string.Empty;
        }

        private void AddFilterParameters(SqlCommand cmd, ErrorLogFilter filter)
        {
            if (!string.IsNullOrEmpty(filter.Platform))
            {
                cmd.Parameters.AddWithValue("@Platform", filter.Platform);
            }

            if (!string.IsNullOrEmpty(filter.AppVersion))
            {
                cmd.Parameters.AddWithValue("@AppVersion", filter.AppVersion);
            }

            if (!string.IsNullOrEmpty(filter.Source))
            {
                cmd.Parameters.AddWithValue("@Source", filter.Source);
            }

            if (!string.IsNullOrEmpty(filter.SearchTerm))
            {
                cmd.Parameters.AddWithValue("@SearchTerm", "%" + filter.SearchTerm + "%");
            }
        }

        private string GetOrderByClause(ErrorLogFilter filter)
        {
            var validSortColumns = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "TotalError", "TOTAL_ERROR" },
                { "Source", "source" },
                { "Platform", "Platform" },
                { "AppVersion", "AppVersion" },
                { "Message", "message" }
            };

            var sortColumn = validSortColumns.ContainsKey(filter.SortBy ?? "TotalError")
                ? validSortColumns[filter.SortBy ?? "TotalError"]
                : "TOTAL_ERROR";

            var direction = filter.SortDescending ? "DESC" : "ASC";

            return $"ORDER BY {sortColumn} {direction}";
        }

        private ErrorLog MapErrorLog(SqlDataReader reader, int id)
        {
            return new ErrorLog
            {
                Id = id,
                Message = reader.IsDBNull(0) ? string.Empty : reader.GetString(0),
                StackTrace = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                Source = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                AppVersion = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                Platform = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
                DeviceInfo = reader.IsDBNull(5) ? string.Empty : reader.GetString(5),
                TotalError = reader.IsDBNull(6) ? 0 : reader.GetInt32(6)
            };
        }
    }
}
