/**
 * Error Log Dashboard JavaScript
 * Handles all dashboard interactions, API calls, and chart rendering
 */

(function () {
    'use strict';

    // Configuration
    const API_BASE_URL = '/api/errorlog';
    const AUTO_REFRESH_INTERVAL = 30000; // 30 seconds

    // State
    let currentPage = 1;
    let currentPageSize = 10;
    let currentSort = 'TotalError';
    let currentSortDesc = true;
    let autoRefreshTimer = null;
    let charts = {};

    // Chart color palette
    const chartColors = [
        'rgba(54, 162, 235, 0.8)',
        'rgba(255, 99, 132, 0.8)',
        'rgba(75, 192, 192, 0.8)',
        'rgba(255, 206, 86, 0.8)',
        'rgba(153, 102, 255, 0.8)',
        'rgba(255, 159, 64, 0.8)',
        'rgba(199, 199, 199, 0.8)',
        'rgba(83, 102, 255, 0.8)',
        'rgba(255, 99, 255, 0.8)',
        'rgba(99, 255, 132, 0.8)'
    ];

    // Platform color map
    const platformColors = {
        'Android': 'rgba(61, 220, 132, 0.8)',
        'iOS': 'rgba(0, 0, 0, 0.8)',
        'Windows': 'rgba(0, 120, 212, 0.8)',
        'Web': 'rgba(255, 152, 0, 0.8)'
    };

    /**
     * Initialize the dashboard
     */
    function init() {
        loadSummary();
        loadChartData();
        loadFilterOptions();
        loadErrorLogs();
        setupEventListeners();
        updateLastUpdated();
    }

    /**
     * Setup all event listeners
     */
    function setupEventListeners() {
        // Auto-refresh toggle
        document.getElementById('autoRefreshToggle').addEventListener('change', function () {
            if (this.checked) {
                startAutoRefresh();
            } else {
                stopAutoRefresh();
            }
        });

        // Filter buttons
        document.getElementById('applyFilters').addEventListener('click', applyFilters);
        document.getElementById('clearFilters').addEventListener('click', clearFilters);
        document.getElementById('searchBtn').addEventListener('click', applyFilters);

        // Search on Enter key
        document.getElementById('searchInput').addEventListener('keypress', function (e) {
            if (e.key === 'Enter') {
                applyFilters();
            }
        });

        // Page size change
        document.getElementById('pageSize').addEventListener('change', function () {
            currentPageSize = parseInt(this.value);
            currentPage = 1;
            loadErrorLogs();
        });

        // Sortable headers
        document.querySelectorAll('.sortable').forEach(function (header) {
            header.addEventListener('click', function () {
                const sortField = this.dataset.sort;
                if (currentSort === sortField) {
                    currentSortDesc = !currentSortDesc;
                } else {
                    currentSort = sortField;
                    currentSortDesc = true;
                }
                updateSortIndicators();
                loadErrorLogs();
            });
        });

        // Export buttons
        document.getElementById('exportCsv').addEventListener('click', function () {
            exportData('csv');
        });
        document.getElementById('exportExcel').addEventListener('click', function () {
            exportData('excel');
        });
    }

    /**
     * Load dashboard summary statistics
     */
    function loadSummary() {
        fetch(API_BASE_URL + '/summary')
            .then(handleResponse)
            .then(function (data) {
                document.getElementById('totalErrors').textContent = formatNumber(data.TotalErrors);
                document.getElementById('uniqueSources').textContent = formatNumber(data.UniqueErrorSources);
                document.getElementById('platformsAffected').textContent = formatNumber(data.AffectedPlatforms);
                document.getElementById('mostAffectedVersion').textContent = data.MostAffectedAppVersion || 'N/A';
            })
            .catch(handleError);
    }

    /**
     * Load all chart data
     */
    function loadChartData() {
        loadPlatformChart();
        loadVersionChart();
        loadSourceChart();
    }

    /**
     * Load platform distribution pie chart
     */
    function loadPlatformChart() {
        fetch(API_BASE_URL + '/platforms')
            .then(handleResponse)
            .then(function (data) {
                const ctx = document.getElementById('platformChart').getContext('2d');

                if (charts.platform) {
                    charts.platform.destroy();
                }

                const labels = data.map(function (d) { return d.Name; });
                const values = data.map(function (d) { return d.Count; });
                const colors = data.map(function (d) {
                    return platformColors[d.Name] || getRandomColor();
                });

                charts.platform = new Chart(ctx, {
                    type: 'doughnut',
                    data: {
                        labels: labels,
                        datasets: [{
                            data: values,
                            backgroundColor: colors,
                            borderWidth: 2,
                            borderColor: '#fff'
                        }]
                    },
                    options: {
                        responsive: true,
                        maintainAspectRatio: false,
                        plugins: {
                            legend: {
                                position: 'right',
                                labels: {
                                    padding: 20,
                                    usePointStyle: true
                                }
                            },
                            tooltip: {
                                callbacks: {
                                    label: function (context) {
                                        const total = context.dataset.data.reduce(function (a, b) { return a + b; }, 0);
                                        const percentage = ((context.raw / total) * 100).toFixed(1);
                                        return context.label + ': ' + formatNumber(context.raw) + ' (' + percentage + '%)';
                                    }
                                }
                            }
                        }
                    }
                });
            })
            .catch(handleError);
    }

    /**
     * Load version distribution bar chart
     */
    function loadVersionChart() {
        fetch(API_BASE_URL + '/versions')
            .then(handleResponse)
            .then(function (data) {
                const ctx = document.getElementById('versionChart').getContext('2d');

                if (charts.version) {
                    charts.version.destroy();
                }

                const labels = data.map(function (d) { return d.Name; });
                const values = data.map(function (d) { return d.Count; });

                charts.version = new Chart(ctx, {
                    type: 'bar',
                    data: {
                        labels: labels,
                        datasets: [{
                            label: 'Error Count',
                            data: values,
                            backgroundColor: 'rgba(75, 192, 192, 0.8)',
                            borderColor: 'rgba(75, 192, 192, 1)',
                            borderWidth: 1
                        }]
                    },
                    options: {
                        responsive: true,
                        maintainAspectRatio: false,
                        plugins: {
                            legend: {
                                display: false
                            }
                        },
                        scales: {
                            y: {
                                beginAtZero: true,
                                ticks: {
                                    callback: function (value) {
                                        return formatNumber(value);
                                    }
                                }
                            }
                        }
                    }
                });
            })
            .catch(handleError);
    }

    /**
     * Load source distribution horizontal bar chart
     */
    function loadSourceChart() {
        fetch(API_BASE_URL + '/sources')
            .then(handleResponse)
            .then(function (data) {
                const ctx = document.getElementById('sourceChart').getContext('2d');

                if (charts.source) {
                    charts.source.destroy();
                }

                const labels = data.map(function (d) {
                    return d.Name.length > 50 ? d.Name.substring(0, 47) + '...' : d.Name;
                });
                const values = data.map(function (d) { return d.Count; });

                charts.source = new Chart(ctx, {
                    type: 'bar',
                    data: {
                        labels: labels,
                        datasets: [{
                            label: 'Error Count',
                            data: values,
                            backgroundColor: chartColors,
                            borderWidth: 0
                        }]
                    },
                    options: {
                        indexAxis: 'y',
                        responsive: true,
                        maintainAspectRatio: false,
                        plugins: {
                            legend: {
                                display: false
                            },
                            tooltip: {
                                callbacks: {
                                    title: function (tooltipItems) {
                                        const index = tooltipItems[0].dataIndex;
                                        return data[index].Name;
                                    }
                                }
                            }
                        },
                        scales: {
                            x: {
                                beginAtZero: true,
                                ticks: {
                                    callback: function (value) {
                                        return formatNumber(value);
                                    }
                                }
                            }
                        }
                    }
                });
            })
            .catch(handleError);
    }

    /**
     * Load filter dropdown options
     */
    function loadFilterOptions() {
        // Load platforms
        fetch(API_BASE_URL + '/filters/Platform')
            .then(handleResponse)
            .then(function (data) {
                populateSelect('filterPlatform', data, 'All Platforms');
            })
            .catch(handleError);

        // Load versions
        fetch(API_BASE_URL + '/filters/AppVersion')
            .then(handleResponse)
            .then(function (data) {
                populateSelect('filterVersion', data, 'All Versions');
            })
            .catch(handleError);

        // Load sources
        fetch(API_BASE_URL + '/filters/source')
            .then(handleResponse)
            .then(function (data) {
                populateSelect('filterSource', data, 'All Sources');
            })
            .catch(handleError);
    }

    /**
     * Load error logs with current filters and pagination
     */
    function loadErrorLogs() {
        const params = new URLSearchParams({
            page: currentPage,
            pageSize: currentPageSize,
            sortBy: currentSort,
            sortDescending: currentSortDesc
        });

        const platform = document.getElementById('filterPlatform').value;
        const version = document.getElementById('filterVersion').value;
        const source = document.getElementById('filterSource').value;
        const search = document.getElementById('searchInput').value;

        if (platform) params.append('platform', platform);
        if (version) params.append('appVersion', version);
        if (source) params.append('source', source);
        if (search) params.append('searchTerm', search);

        showTableLoading();

        fetch(API_BASE_URL + '?' + params.toString())
            .then(handleResponse)
            .then(function (data) {
                renderErrorTable(data);
                renderPagination(data);
                updatePaginationInfo(data);
            })
            .catch(handleError);
    }

    /**
     * Render the error log table
     */
    function renderErrorTable(data) {
        const tbody = document.getElementById('errorTableBody');

        if (!data.Data || data.Data.length === 0) {
            tbody.innerHTML = '<tr><td colspan="7" class="empty-state">' +
                '<i class="bi bi-inbox"></i>' +
                '<h5>No errors found</h5>' +
                '<p>Try adjusting your filters or search criteria</p>' +
                '</td></tr>';
            return;
        }

        var html = '';
        data.Data.forEach(function (error) {
            html += '<tr onclick="dashboard.showErrorDetail(' + error.Id + ')">' +
                '<td><span class="error-count ' + getErrorCountClass(error.TotalError) + '">' + formatNumber(error.TotalError) + '</span></td>' +
                '<td><span class="badge platform-badge ' + getPlatformClass(error.Platform) + '">' + escapeHtml(error.Platform || 'Unknown') + '</span></td>' +
                '<td><span class="badge bg-secondary">' + escapeHtml(error.AppVersion || 'N/A') + '</span></td>' +
                '<td><span class="source-badge" title="' + escapeHtml(error.Source) + '">' + escapeHtml(error.Source || 'Unknown') + '</span></td>' +
                '<td><div class="message-preview" title="' + escapeHtml(error.Message) + '">' + escapeHtml(error.Message || 'No message') + '</div></td>' +
                '<td>' + escapeHtml(error.DeviceInfo || 'N/A') + '</td>' +
                '<td><button class="btn btn-sm btn-outline-primary btn-view-detail" onclick="event.stopPropagation(); dashboard.showErrorDetail(' + error.Id + ');">' +
                '<i class="bi bi-eye"></i></button></td>' +
                '</tr>';
        });

        tbody.innerHTML = html;
    }

    /**
     * Render pagination controls
     */
    function renderPagination(data) {
        const pagination = document.getElementById('pagination');
        var html = '';

        // Previous button
        html += '<li class="page-item ' + (data.Page <= 1 ? 'disabled' : '') + '">' +
            '<a class="page-link" href="#" onclick="dashboard.goToPage(' + (data.Page - 1) + '); return false;">&laquo;</a></li>';

        // Page numbers
        const startPage = Math.max(1, data.Page - 2);
        const endPage = Math.min(data.TotalPages, data.Page + 2);

        if (startPage > 1) {
            html += '<li class="page-item"><a class="page-link" href="#" onclick="dashboard.goToPage(1); return false;">1</a></li>';
            if (startPage > 2) {
                html += '<li class="page-item disabled"><span class="page-link">...</span></li>';
            }
        }

        for (var i = startPage; i <= endPage; i++) {
            html += '<li class="page-item ' + (i === data.Page ? 'active' : '') + '">' +
                '<a class="page-link" href="#" onclick="dashboard.goToPage(' + i + '); return false;">' + i + '</a></li>';
        }

        if (endPage < data.TotalPages) {
            if (endPage < data.TotalPages - 1) {
                html += '<li class="page-item disabled"><span class="page-link">...</span></li>';
            }
            html += '<li class="page-item"><a class="page-link" href="#" onclick="dashboard.goToPage(' + data.TotalPages + '); return false;">' + data.TotalPages + '</a></li>';
        }

        // Next button
        html += '<li class="page-item ' + (data.Page >= data.TotalPages ? 'disabled' : '') + '">' +
            '<a class="page-link" href="#" onclick="dashboard.goToPage(' + (data.Page + 1) + '); return false;">&raquo;</a></li>';

        pagination.innerHTML = html;
    }

    /**
     * Update pagination info text
     */
    function updatePaginationInfo(data) {
        const start = (data.Page - 1) * data.PageSize + 1;
        const end = Math.min(data.Page * data.PageSize, data.TotalCount);
        document.getElementById('paginationInfo').textContent =
            'Showing ' + (data.TotalCount > 0 ? start : 0) + ' to ' + end + ' of ' + formatNumber(data.TotalCount) + ' entries';
    }

    /**
     * Go to specific page
     */
    function goToPage(page) {
        currentPage = page;
        loadErrorLogs();
    }

    /**
     * Show error detail modal
     */
    function showErrorDetail(id) {
        fetch(API_BASE_URL + '/' + id)
            .then(handleResponse)
            .then(function (error) {
                document.getElementById('detailPlatform').textContent = error.Platform || 'Unknown';
                document.getElementById('detailVersion').textContent = error.AppVersion || 'N/A';
                document.getElementById('detailCount').textContent = formatNumber(error.TotalError);
                document.getElementById('detailSource').textContent = error.Source || 'Unknown';
                document.getElementById('detailDevice').textContent = error.DeviceInfo || 'N/A';
                document.getElementById('detailMessage').textContent = error.Message || 'No message';
                document.getElementById('detailStackTrace').textContent = formatStackTrace(error.StackTrace);

                var modal = new bootstrap.Modal(document.getElementById('errorDetailModal'));
                modal.show();
            })
            .catch(handleError);
    }

    /**
     * Apply filters
     */
    function applyFilters() {
        currentPage = 1;
        loadErrorLogs();
    }

    /**
     * Clear all filters
     */
    function clearFilters() {
        document.getElementById('filterPlatform').value = '';
        document.getElementById('filterVersion').value = '';
        document.getElementById('filterSource').value = '';
        document.getElementById('searchInput').value = '';
        currentPage = 1;
        loadErrorLogs();
    }

    /**
     * Export data to CSV or Excel
     */
    function exportData(format) {
        const params = new URLSearchParams({
            page: 1,
            pageSize: 10000 // Get all data for export
        });

        const platform = document.getElementById('filterPlatform').value;
        const version = document.getElementById('filterVersion').value;
        const source = document.getElementById('filterSource').value;
        const search = document.getElementById('searchInput').value;

        if (platform) params.append('platform', platform);
        if (version) params.append('appVersion', version);
        if (source) params.append('source', source);
        if (search) params.append('searchTerm', search);

        fetch(API_BASE_URL + '?' + params.toString())
            .then(handleResponse)
            .then(function (data) {
                if (format === 'csv') {
                    downloadCsv(data.Data);
                } else {
                    downloadExcel(data.Data);
                }
            })
            .catch(handleError);
    }

    /**
     * Download data as CSV
     */
    function downloadCsv(data) {
        const headers = ['Total Errors', 'Platform', 'App Version', 'Source', 'Message', 'Device Info', 'Stack Trace'];
        const csvContent = [
            headers.join(','),
            ...data.map(function (row) {
                return [
                    row.TotalError,
                    '"' + escapeCsvField(row.Platform) + '"',
                    '"' + escapeCsvField(row.AppVersion) + '"',
                    '"' + escapeCsvField(row.Source) + '"',
                    '"' + escapeCsvField(row.Message) + '"',
                    '"' + escapeCsvField(row.DeviceInfo) + '"',
                    '"' + escapeCsvField(row.StackTrace) + '"'
                ].join(',');
            })
        ].join('\n');

        downloadFile(csvContent, 'error_logs_' + getTimestamp() + '.csv', 'text/csv');
    }

    /**
     * Download data as Excel (using CSV with proper encoding)
     */
    function downloadExcel(data) {
        // For simplicity, we create a tab-separated file that Excel can open
        const headers = ['Total Errors', 'Platform', 'App Version', 'Source', 'Message', 'Device Info'];
        const content = [
            headers.join('\t'),
            ...data.map(function (row) {
                return [
                    row.TotalError,
                    row.Platform || '',
                    row.AppVersion || '',
                    row.Source || '',
                    (row.Message || '').replace(/[\t\n\r]/g, ' '),
                    row.DeviceInfo || ''
                ].join('\t');
            })
        ].join('\n');

        // Add BOM for Excel UTF-8 detection
        const bom = '\uFEFF';
        downloadFile(bom + content, 'error_logs_' + getTimestamp() + '.xls', 'application/vnd.ms-excel');
    }

    /**
     * Download file helper
     */
    function downloadFile(content, filename, mimeType) {
        const blob = new Blob([content], { type: mimeType + ';charset=utf-8' });
        const link = document.createElement('a');
        link.href = URL.createObjectURL(blob);
        link.download = filename;
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
        URL.revokeObjectURL(link.href);
    }

    /**
     * Start auto-refresh
     */
    function startAutoRefresh() {
        stopAutoRefresh();
        autoRefreshTimer = setInterval(function () {
            refreshDashboard();
        }, AUTO_REFRESH_INTERVAL);
    }

    /**
     * Stop auto-refresh
     */
    function stopAutoRefresh() {
        if (autoRefreshTimer) {
            clearInterval(autoRefreshTimer);
            autoRefreshTimer = null;
        }
    }

    /**
     * Refresh all dashboard data
     */
    function refreshDashboard() {
        loadSummary();
        loadChartData();
        loadErrorLogs();
        updateLastUpdated();
    }

    /**
     * Update last updated timestamp
     */
    function updateLastUpdated() {
        const now = new Date();
        document.getElementById('lastUpdated').textContent =
            'Last updated: ' + now.toLocaleTimeString();
    }

    // Helper functions

    function handleResponse(response) {
        if (!response.ok) {
            throw new Error('Network response was not ok');
        }
        return response.json();
    }

    function handleError(error) {
        console.error('Error:', error);
        // Show user-friendly error message
        alert('An error occurred while loading data. Please try again.');
    }

    function formatNumber(num) {
        if (num === undefined || num === null) return '0';
        return num.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ',');
    }

    function escapeHtml(text) {
        if (!text) return '';
        const div = document.createElement('div');
        div.textContent = text;
        return div.innerHTML;
    }

    function escapeCsvField(text) {
        if (!text) return '';
        return text.replace(/"/g, '""').replace(/[\n\r]/g, ' ');
    }

    function formatStackTrace(stackTrace) {
        if (!stackTrace) return 'No stack trace available';
        // Format the stack trace for better readability
        return stackTrace
            .replace(/\[Exception Level (\d+)\]/g, '\n[Exception Level $1]\n')
            .replace(/at /g, '\n  at ')
            .trim();
    }

    function getErrorCountClass(count) {
        if (count >= 100) return 'high';
        if (count >= 10) return 'medium';
        return 'low';
    }

    function getPlatformClass(platform) {
        if (!platform) return 'platform-default';
        const p = platform.toLowerCase();
        if (p.includes('android')) return 'platform-android';
        if (p.includes('ios')) return 'platform-ios';
        if (p.includes('windows')) return 'platform-windows';
        if (p.includes('web')) return 'platform-web';
        return 'platform-default';
    }

    function populateSelect(selectId, options, defaultText) {
        const select = document.getElementById(selectId);
        select.innerHTML = '<option value="">' + defaultText + '</option>';
        options.forEach(function (option) {
            const opt = document.createElement('option');
            opt.value = option;
            opt.textContent = option;
            select.appendChild(opt);
        });
    }

    function showTableLoading() {
        document.getElementById('errorTableBody').innerHTML =
            '<tr><td colspan="7" class="text-center py-5">' +
            '<div class="spinner-border text-primary" role="status">' +
            '<span class="visually-hidden">Loading...</span></div>' +
            '<p class="mt-2 mb-0 text-muted">Loading error logs...</p></td></tr>';
    }

    function updateSortIndicators() {
        document.querySelectorAll('.sortable').forEach(function (header) {
            header.classList.remove('sorted-asc', 'sorted-desc');
            if (header.dataset.sort === currentSort) {
                header.classList.add(currentSortDesc ? 'sorted-desc' : 'sorted-asc');
            }
        });
    }

    function getTimestamp() {
        const now = new Date();
        return now.getFullYear().toString() +
            String(now.getMonth() + 1).padStart(2, '0') +
            String(now.getDate()).padStart(2, '0') + '_' +
            String(now.getHours()).padStart(2, '0') +
            String(now.getMinutes()).padStart(2, '0');
    }

    function getRandomColor() {
        return chartColors[Math.floor(Math.random() * chartColors.length)];
    }

    // Initialize on DOM ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }

    // Expose public methods
    window.dashboard = {
        goToPage: goToPage,
        showErrorDetail: showErrorDetail,
        refresh: refreshDashboard
    };
})();
