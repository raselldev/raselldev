using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using ErrorLogDashboard.Web.Models;
using ErrorLogDashboard.Web.Services;

namespace ErrorLogDashboard.Web.Controllers
{
    /// <summary>
    /// API Controller for Error Log Dashboard operations
    /// </summary>
    [RoutePrefix("api/errorlog")]
    public class ErrorLogController : ApiController
    {
        private readonly ErrorLogService _service;

        public ErrorLogController()
        {
            _service = new ErrorLogService();
        }

        /// <summary>
        /// Get all errors with optional filters
        /// GET /api/errorlog?platform=Android&amp;appVersion=2.0.0&amp;page=1&amp;pageSize=10
        /// </summary>
        [HttpGet]
        [Route("")]
        public IHttpActionResult GetErrorLogs(
            [FromUri] string platform = null,
            [FromUri] string appVersion = null,
            [FromUri] string source = null,
            [FromUri] string searchTerm = null,
            [FromUri] int page = 1,
            [FromUri] int pageSize = 10,
            [FromUri] string sortBy = "TotalError",
            [FromUri] bool sortDescending = true)
        {
            try
            {
                var filter = new ErrorLogFilter
                {
                    Platform = platform,
                    AppVersion = appVersion,
                    Source = source,
                    SearchTerm = searchTerm,
                    Page = page,
                    PageSize = Math.Min(pageSize, 100), // Limit max page size
                    SortBy = sortBy,
                    SortDescending = sortDescending
                };

                var result = _service.GetErrorLogs(filter);
                return Ok(result);
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
        public IHttpActionResult GetSummary()
        {
            try
            {
                var summary = _service.GetSummary();
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
        public IHttpActionResult GetPlatformStats()
        {
            try
            {
                var stats = _service.GetPlatformStats();
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
        public IHttpActionResult GetVersionStats()
        {
            try
            {
                var stats = _service.GetVersionStats();
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
        public IHttpActionResult GetSourceStats()
        {
            try
            {
                var stats = _service.GetSourceStats();
                return Ok(stats);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// Get specific error details by ID
        /// GET /api/errorlog/{id}
        /// </summary>
        [HttpGet]
        [Route("{id:int}")]
        public IHttpActionResult GetErrorById(int id)
        {
            try
            {
                var error = _service.GetErrorById(id);
                if (error == null)
                {
                    return NotFound();
                }
                return Ok(error);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// Get distinct values for filter dropdowns
        /// GET /api/errorlog/filters/{column}
        /// </summary>
        [HttpGet]
        [Route("filters/{column}")]
        public IHttpActionResult GetFilterValues(string column)
        {
            try
            {
                var values = _service.GetDistinctValues(column);
                return Ok(values);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }
    }
}
