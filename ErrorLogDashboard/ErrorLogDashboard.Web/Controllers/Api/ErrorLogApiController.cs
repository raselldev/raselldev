using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using ErrorLogDashboard.Web.Models;
using ErrorLogDashboard.Web.Services;

namespace ErrorLogDashboard.Web.Controllers.Api
{
    /// <summary>
    /// Web API controller for error log operations
    /// </summary>
    [RoutePrefix("api/errorlog")]
    public class ErrorLogApiController : ApiController
    {
        private readonly IErrorLogService _errorLogService;

        public ErrorLogApiController()
        {
            _errorLogService = new ErrorLogService();
        }

        public ErrorLogApiController(IErrorLogService errorLogService)
        {
            _errorLogService = errorLogService;
        }

        #region Read Operations

        /// <summary>
        /// Get all errors with optional filters
        /// GET /api/errorlog
        /// </summary>
        [HttpGet]
        [Route("")]
        public async Task<IHttpActionResult> GetErrorLogs(
            [FromUri] string platform = null,
            [FromUri] string appVersion = null,
            [FromUri] string source = null,
            [FromUri] bool? isResolved = null,
            [FromUri] DateTime? startDate = null,
            [FromUri] DateTime? endDate = null,
            [FromUri] string search = null,
            [FromUri] int page = 1,
            [FromUri] int pageSize = 10,
            [FromUri] string sortColumn = "Timestamp",
            [FromUri] string sortDirection = "DESC")
        {
            try
            {
                var filter = new ErrorLogFilter
                {
                    Platform = platform,
                    AppVersion = appVersion,
                    Source = source,
                    IsResolved = isResolved,
                    StartDate = startDate,
                    EndDate = endDate,
                    Search = search,
                    Page = Math.Max(1, page),
                    PageSize = Math.Min(Math.Max(1, pageSize), 100),
                    SortColumn = sortColumn,
                    SortDirection = sortDirection
                };

                var result = await _errorLogService.GetErrorLogsAsync(filter);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// Get specific error details
        /// GET /api/errorlog/{id}
        /// </summary>
        [HttpGet]
        [Route("{id:int}")]
        public async Task<IHttpActionResult> GetErrorLog(int id)
        {
            try
            {
                var errorLog = await _errorLogService.GetErrorLogByIdAsync(id);
                if (errorLog == null)
                    return NotFound();

                return Ok(errorLog);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// Get dashboard summary statistics
        /// GET /api/errorlog/summary
        /// </summary>
        [HttpGet]
        [Route("summary")]
        public async Task<IHttpActionResult> GetSummary()
        {
            try
            {
                var summary = await _errorLogService.GetSummaryAsync();
                return Ok(summary);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// Get error count grouped by platform
        /// GET /api/errorlog/platforms
        /// </summary>
        [HttpGet]
        [Route("platforms")]
        public async Task<IHttpActionResult> GetPlatformStats()
        {
            try
            {
                var stats = await _errorLogService.GetPlatformStatsAsync();
                return Ok(stats);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// Get error count grouped by app version
        /// GET /api/errorlog/versions
        /// </summary>
        [HttpGet]
        [Route("versions")]
        public async Task<IHttpActionResult> GetVersionStats()
        {
            try
            {
                var stats = await _errorLogService.GetVersionStatsAsync();
                return Ok(stats);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// Get error count grouped by source (top 10)
        /// GET /api/errorlog/sources
        /// </summary>
        [HttpGet]
        [Route("sources")]
        public async Task<IHttpActionResult> GetSourceStats([FromUri] int top = 10)
        {
            try
            {
                var stats = await _errorLogService.GetTopSourcesAsync(top);
                return Ok(stats);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// Get error count grouped by date (trends)
        /// GET /api/errorlog/trends
        /// </summary>
        [HttpGet]
        [Route("trends")]
        public async Task<IHttpActionResult> GetTrends([FromUri] int days = 30)
        {
            try
            {
                var stats = await _errorLogService.GetTrendsAsync(days);
                return Ok(stats);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// Get resolved vs unresolved counts
        /// GET /api/errorlog/resolution-stats
        /// </summary>
        [HttpGet]
        [Route("resolution-stats")]
        public async Task<IHttpActionResult> GetResolutionStats()
        {
            try
            {
                var stats = await _errorLogService.GetResolutionStatsAsync();
                return Ok(stats);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// Get distinct platforms for filter dropdown
        /// GET /api/errorlog/filter-options/platforms
        /// </summary>
        [HttpGet]
        [Route("filter-options/platforms")]
        public async Task<IHttpActionResult> GetDistinctPlatforms()
        {
            try
            {
                var platforms = await _errorLogService.GetDistinctPlatformsAsync();
                return Ok(platforms);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// Get distinct versions for filter dropdown
        /// GET /api/errorlog/filter-options/versions
        /// </summary>
        [HttpGet]
        [Route("filter-options/versions")]
        public async Task<IHttpActionResult> GetDistinctVersions()
        {
            try
            {
                var versions = await _errorLogService.GetDistinctVersionsAsync();
                return Ok(versions);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// Get distinct sources for filter dropdown
        /// GET /api/errorlog/filter-options/sources
        /// </summary>
        [HttpGet]
        [Route("filter-options/sources")]
        public async Task<IHttpActionResult> GetDistinctSources()
        {
            try
            {
                var sources = await _errorLogService.GetDistinctSourcesAsync();
                return Ok(sources);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        #endregion

        #region Write Operations

        /// <summary>
        /// Mark single error as resolved
        /// PUT /api/errorlog/{id}/resolve
        /// </summary>
        [HttpPut]
        [Route("{id:int}/resolve")]
        public async Task<IHttpActionResult> Resolve(int id)
        {
            try
            {
                var success = await _errorLogService.ResolveAsync(id);
                if (!success)
                    return NotFound();

                return Ok(new { success = true, message = "Error marked as resolved." });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// Mark single error as unresolved
        /// PUT /api/errorlog/{id}/unresolve
        /// </summary>
        [HttpPut]
        [Route("{id:int}/unresolve")]
        public async Task<IHttpActionResult> Unresolve(int id)
        {
            try
            {
                var success = await _errorLogService.UnresolveAsync(id);
                if (!success)
                    return NotFound();

                return Ok(new { success = true, message = "Error marked as unresolved." });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// Mark multiple errors as resolved
        /// PUT /api/errorlog/bulk-resolve
        /// </summary>
        [HttpPut]
        [Route("bulk-resolve")]
        public async Task<IHttpActionResult> BulkResolve([FromBody] BulkOperationRequest request)
        {
            try
            {
                if (request?.Ids == null || request.Ids.Length == 0)
                    return BadRequest("No IDs provided.");

                var affected = await _errorLogService.BulkResolveAsync(request.Ids);
                return Ok(new { success = true, affected = affected, message = $"{affected} error(s) marked as resolved." });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// Mark multiple errors as unresolved
        /// PUT /api/errorlog/bulk-unresolve
        /// </summary>
        [HttpPut]
        [Route("bulk-unresolve")]
        public async Task<IHttpActionResult> BulkUnresolve([FromBody] BulkOperationRequest request)
        {
            try
            {
                if (request?.Ids == null || request.Ids.Length == 0)
                    return BadRequest("No IDs provided.");

                var affected = await _errorLogService.BulkUnresolveAsync(request.Ids);
                return Ok(new { success = true, affected = affected, message = $"{affected} error(s) marked as unresolved." });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        #endregion
    }
}
