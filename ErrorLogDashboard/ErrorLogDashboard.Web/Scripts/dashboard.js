/**
 * Error Log Dashboard JavaScript
 * Handles all client-side functionality including charts, data table, and API interactions
 */

// Global variables
let errorLogTable = null;
let platformChart = null;
let resolutionChart = null;
let versionChart = null;
let sourceChart = null;
let trendChart = null;
let autoRefreshInterval = null;
let currentErrorId = null;
let selectedIds = [];

// Configuration
const config = {
    autoRefreshIntervalMs: 30000, // 30 seconds - can be configured
    apiBase: '/api/errorlog'
};

// Chart colors
const chartColors = {
    primary: '#0d6efd',
    success: '#198754',
    danger: '#dc3545',
    warning: '#ffc107',
    info: '#0dcaf0',
    purple: '#6f42c1',
    pink: '#d63384',
    orange: '#fd7e14',
    teal: '#20c997',
    cyan: '#0dcaf0'
};

const platformColors = {
    'Android': '#3DDC84',
    'iOS': '#999999',
    'Windows': '#0078D6',
    'Web': '#ff6600',
    'Other': '#6c757d'
};

/**
 * Initialize dashboard on document ready
 */
$(document).ready(function() {
    initializeToastr();
    initializeDatePickers();
    initializeDataTable();
    initializeCharts();
    initializeEventHandlers();
    loadDashboardData();
    
    // Check for saved theme preference
    const savedTheme = localStorage.getItem('theme');
    if (savedTheme === 'dark') {
        $('body').removeClass('light-mode').addClass('dark-mode');
        updateChartTheme();
    }
});

/**
 * Initialize Toastr notifications
 */
function initializeToastr() {
    toastr.options = {
        closeButton: true,
        progressBar: true,
        positionClass: 'toast-top-right',
        timeOut: 3000,
        extendedTimeOut: 1000
    };
}

/**
 * Initialize Flatpickr date pickers
 */
function initializeDatePickers() {
    flatpickr('#filterStartDate', {
        dateFormat: 'Y-m-d',
        allowInput: true,
        onChange: function() {
            reloadTable();
        }
    });
    
    flatpickr('#filterEndDate', {
        dateFormat: 'Y-m-d',
        allowInput: true,
        onChange: function() {
            reloadTable();
        }
    });
}

/**
 * Initialize DataTable
 */
function initializeDataTable() {
    errorLogTable = $('#errorLogTable').DataTable({
        processing: true,
        serverSide: true,
        ajax: {
            url: config.apiBase,
            type: 'GET',
            data: function(d) {
                return {
                    page: Math.floor(d.start / d.length) + 1,
                    pageSize: d.length,
                    search: d.search.value,
                    sortColumn: d.columns[d.order[0].column].data || 'Timestamp',
                    sortDirection: d.order[0].dir.toUpperCase(),
                    platform: $('#filterPlatform').val(),
                    appVersion: $('#filterVersion').val(),
                    source: $('#filterSource').val(),
                    isResolved: $('#filterStatus').val() || null,
                    startDate: $('#filterStartDate').val() || null,
                    endDate: $('#filterEndDate').val() || null
                };
            },
            dataSrc: function(json) {
                json.recordsTotal = json.totalCount;
                json.recordsFiltered = json.totalCount;
                return json.items || [];
            }
        },
        columns: [
            {
                data: 'id',
                orderable: false,
                searchable: false,
                render: function(data) {
                    return '<input type="checkbox" class="form-check-input row-checkbox" value="' + data + '" />';
                }
            },
            {
                data: 'isResolved',
                render: function(data) {
                    if (data) {
                        return '<span class="status-badge resolved"><i class="bi bi-check-circle-fill"></i> Resolved</span>';
                    }
                    return '<span class="status-badge unresolved"><i class="bi bi-x-circle-fill"></i> Unresolved</span>';
                }
            },
            {
                data: 'timestamp',
                render: function(data) {
                    if (!data) return '-';
                    const date = new Date(data);
                    return date.toLocaleDateString() + ' ' + date.toLocaleTimeString();
                }
            },
            {
                data: 'message',
                render: function(data) {
                    if (!data) return '-';
                    const truncated = data.length > 80 ? data.substring(0, 80) + '...' : data;
                    return '<span class="message-cell" title="' + escapeHtml(data) + '">' + escapeHtml(truncated) + '</span>';
                }
            },
            {
                data: 'source',
                render: function(data) {
                    if (!data) return '-';
                    const truncated = data.length > 30 ? data.substring(0, 30) + '...' : data;
                    return '<span class="source-cell" title="' + escapeHtml(data) + '">' + escapeHtml(truncated) + '</span>';
                }
            },
            {
                data: 'platform',
                render: function(data) {
                    if (!data) return '-';
                    let icon = 'bi-phone';
                    let colorClass = '';
                    if (data.toLowerCase() === 'android') {
                        icon = 'bi-android2';
                        colorClass = 'platform-android';
                    } else if (data.toLowerCase() === 'ios') {
                        icon = 'bi-apple';
                        colorClass = 'platform-ios';
                    } else if (data.toLowerCase() === 'windows') {
                        icon = 'bi-windows';
                        colorClass = 'platform-windows';
                    }
                    return '<i class="bi ' + icon + ' platform-icon ' + colorClass + '"></i>' + escapeHtml(data);
                }
            },
            {
                data: 'appVersion',
                render: function(data) {
                    if (!data) return '-';
                    return '<span class="version-badge">' + escapeHtml(data) + '</span>';
                }
            },
            {
                data: 'deviceInfo',
                render: function(data) {
                    if (!data) return '-';
                    const truncated = data.length > 20 ? data.substring(0, 20) + '...' : data;
                    return '<span class="device-badge" title="' + escapeHtml(data) + '">' + escapeHtml(truncated) + '</span>';
                }
            },
            {
                data: 'id',
                orderable: false,
                searchable: false,
                render: function(data, type, row) {
                    let buttons = '<div class="action-buttons">';
                    buttons += '<button class="btn btn-sm btn-outline-primary btn-action view-btn" data-id="' + data + '" title="View Details"><i class="bi bi-eye"></i></button>';
                    if (row.isResolved) {
                        buttons += '<button class="btn btn-sm btn-outline-danger btn-action unresolve-btn" data-id="' + data + '" title="Mark as Unresolved"><i class="bi bi-x-circle"></i></button>';
                    } else {
                        buttons += '<button class="btn btn-sm btn-outline-success btn-action resolve-btn" data-id="' + data + '" title="Mark as Resolved"><i class="bi bi-check-circle"></i></button>';
                    }
                    buttons += '</div>';
                    return buttons;
                }
            }
        ],
        order: [[2, 'desc']],
        pageLength: 10,
        lengthMenu: [[10, 25, 50, 100], [10, 25, 50, 100]],
        dom: "<'row'<'col-sm-12 col-md-6'l><'col-sm-12 col-md-6'f>>" +
             "<'row'<'col-sm-12'tr>>" +
             "<'row'<'col-sm-12 col-md-5'i><'col-sm-12 col-md-7'p>>",
        language: {
            processing: '<div class="spinner-border text-primary" role="status"><span class="visually-hidden">Loading...</span></div>',
            emptyTable: 'No error logs found',
            zeroRecords: 'No matching error logs found'
        },
        drawCallback: function() {
            updateBulkActionButtons();
        }
    });
}

/**
 * Initialize all charts
 */
function initializeCharts() {
    // Platform Distribution Chart
    const platformCtx = document.getElementById('platformChart').getContext('2d');
    platformChart = new Chart(platformCtx, {
        type: 'doughnut',
        data: {
            labels: [],
            datasets: [{
                data: [],
                backgroundColor: Object.values(platformColors),
                borderWidth: 2,
                borderColor: '#fff'
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: true,
            plugins: {
                legend: {
                    position: 'bottom',
                    labels: { padding: 15 }
                }
            }
        }
    });

    // Resolution Status Chart
    const resolutionCtx = document.getElementById('resolutionChart').getContext('2d');
    resolutionChart = new Chart(resolutionCtx, {
        type: 'doughnut',
        data: {
            labels: ['Resolved', 'Unresolved'],
            datasets: [{
                data: [0, 0],
                backgroundColor: [chartColors.success, chartColors.danger],
                borderWidth: 2,
                borderColor: '#fff'
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: true,
            plugins: {
                legend: {
                    position: 'bottom',
                    labels: { padding: 15 }
                }
            }
        }
    });

    // Version Distribution Chart
    const versionCtx = document.getElementById('versionChart').getContext('2d');
    versionChart = new Chart(versionCtx, {
        type: 'bar',
        data: {
            labels: [],
            datasets: [{
                label: 'Errors',
                data: [],
                backgroundColor: chartColors.primary,
                borderColor: chartColors.primary,
                borderWidth: 1
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: true,
            indexAxis: 'y',
            plugins: {
                legend: { display: false }
            },
            scales: {
                x: { beginAtZero: true }
            }
        }
    });

    // Top Sources Chart
    const sourceCtx = document.getElementById('sourceChart').getContext('2d');
    sourceChart = new Chart(sourceCtx, {
        type: 'bar',
        data: {
            labels: [],
            datasets: [{
                label: 'Errors',
                data: [],
                backgroundColor: chartColors.warning,
                borderColor: chartColors.warning,
                borderWidth: 1
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: true,
            indexAxis: 'y',
            plugins: {
                legend: { display: false }
            },
            scales: {
                x: { beginAtZero: true }
            }
        }
    });

    // Trend Chart
    const trendCtx = document.getElementById('trendChart').getContext('2d');
    trendChart = new Chart(trendCtx, {
        type: 'line',
        data: {
            labels: [],
            datasets: [{
                label: 'Errors',
                data: [],
                borderColor: chartColors.primary,
                backgroundColor: 'rgba(13, 110, 253, 0.1)',
                fill: true,
                tension: 0.4
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: { display: false }
            },
            scales: {
                y: { beginAtZero: true }
            }
        }
    });
}

/**
 * Initialize event handlers
 */
function initializeEventHandlers() {
    // Filter changes
    $('#filterPlatform, #filterVersion, #filterSource, #filterStatus').on('change', function() {
        reloadTable();
    });

    // Reset filters
    $('#resetFilters').on('click', function() {
        $('#filterPlatform, #filterVersion, #filterSource').val('');
        $('#filterStatus').val('false'); // Default to unresolved
        $('#filterStartDate, #filterEndDate').val('');
        reloadTable();
    });

    // Status tabs
    $('#statusTabs .nav-link').on('click', function() {
        $('#statusTabs .nav-link').removeClass('active');
        $(this).addClass('active');
        const status = $(this).data('status');
        $('#filterStatus').val(status === '' ? '' : status);
        reloadTable();
    });

    // Select all checkbox
    $('#selectAll').on('change', function() {
        const isChecked = $(this).prop('checked');
        $('.row-checkbox').prop('checked', isChecked);
        updateSelectedIds();
    });

    // Row checkbox change (delegated)
    $('#errorLogTable tbody').on('change', '.row-checkbox', function() {
        updateSelectedIds();
    });

    // View button click (delegated)
    $('#errorLogTable tbody').on('click', '.view-btn', function(e) {
        e.stopPropagation();
        const id = $(this).data('id');
        showErrorDetail(id);
    });

    // Resolve button click (delegated)
    $('#errorLogTable tbody').on('click', '.resolve-btn', function(e) {
        e.stopPropagation();
        const id = $(this).data('id');
        resolveError(id);
    });

    // Unresolve button click (delegated)
    $('#errorLogTable tbody').on('click', '.unresolve-btn', function(e) {
        e.stopPropagation();
        const id = $(this).data('id');
        unresolveError(id);
    });

    // Row click to show details (delegated)
    $('#errorLogTable tbody').on('click', 'tr', function(e) {
        if ($(e.target).is('input, button, i') || $(e.target).closest('button').length) {
            return;
        }
        const data = errorLogTable.row(this).data();
        if (data && data.id) {
            showErrorDetail(data.id);
        }
    });

    // Bulk resolve
    $('#bulkResolve').on('click', function() {
        if (selectedIds.length === 0) return;
        showConfirmDialog(
            'Resolve Selected Errors',
            `Are you sure you want to mark ${selectedIds.length} error(s) as resolved?`,
            function() {
                bulkResolve(selectedIds);
            }
        );
    });

    // Bulk unresolve
    $('#bulkUnresolve').on('click', function() {
        if (selectedIds.length === 0) return;
        showConfirmDialog(
            'Unresolve Selected Errors',
            `Are you sure you want to mark ${selectedIds.length} error(s) as unresolved?`,
            function() {
                bulkUnresolve(selectedIds);
            }
        );
    });

    // Modal resolve button
    $('#modalResolveBtn').on('click', function() {
        if (currentErrorId) {
            resolveError(currentErrorId);
        }
    });

    // Modal unresolve button
    $('#modalUnresolveBtn').on('click', function() {
        if (currentErrorId) {
            unresolveError(currentErrorId);
        }
    });

    // Dark mode toggle
    $('#darkModeToggle').on('click', function() {
        $('body').toggleClass('light-mode dark-mode');
        const isDark = $('body').hasClass('dark-mode');
        localStorage.setItem('theme', isDark ? 'dark' : 'light');
        $(this).find('i').toggleClass('bi-moon-fill bi-sun-fill');
        updateChartTheme();
    });

    // Auto-refresh toggle
    $('#autoRefreshToggle').on('change', function() {
        if ($(this).prop('checked')) {
            startAutoRefresh();
            toastr.info('Auto-refresh enabled (every 30 seconds)');
        } else {
            stopAutoRefresh();
            toastr.info('Auto-refresh disabled');
        }
    });
}

/**
 * Load all dashboard data
 */
function loadDashboardData() {
    loadSummary();
    loadPlatformStats();
    loadResolutionStats();
    loadVersionStats();
    loadSourceStats();
    loadTrendStats();
}

/**
 * Load summary statistics
 */
function loadSummary() {
    $.ajax({
        url: config.apiBase + '/summary',
        method: 'GET',
        success: function(data) {
            $('#totalErrors').text(data.totalErrors || 0);
            $('#unresolvedErrors').text(data.unresolvedErrors || 0);
            $('#resolvedErrors').text(data.resolvedErrors || 0);
            $('#uniqueSources').text(data.uniqueErrorSources || 0);
            $('#affectedPlatforms').text(data.affectedPlatforms || 0);
            $('#unresolvedBadge').text(data.unresolvedErrors || 0);
            $('#resolvedBadge').text(data.resolvedErrors || 0);
        },
        error: function() {
            console.error('Failed to load summary');
        }
    });
}

/**
 * Load platform statistics for chart
 */
function loadPlatformStats() {
    $.ajax({
        url: config.apiBase + '/platforms',
        method: 'GET',
        success: function(data) {
            const labels = data.map(d => d.platform);
            const values = data.map(d => d.count);
            const colors = labels.map(l => platformColors[l] || chartColors.info);
            
            platformChart.data.labels = labels;
            platformChart.data.datasets[0].data = values;
            platformChart.data.datasets[0].backgroundColor = colors;
            platformChart.update();
        },
        error: function() {
            console.error('Failed to load platform stats');
        }
    });
}

/**
 * Load resolution statistics for chart
 */
function loadResolutionStats() {
    $.ajax({
        url: config.apiBase + '/resolution-stats',
        method: 'GET',
        success: function(data) {
            resolutionChart.data.datasets[0].data = [data.resolved || 0, data.unresolved || 0];
            resolutionChart.update();
        },
        error: function() {
            console.error('Failed to load resolution stats');
        }
    });
}

/**
 * Load version statistics for chart
 */
function loadVersionStats() {
    $.ajax({
        url: config.apiBase + '/versions',
        method: 'GET',
        success: function(data) {
            const topVersions = data.slice(0, 8);
            versionChart.data.labels = topVersions.map(d => d.appVersion);
            versionChart.data.datasets[0].data = topVersions.map(d => d.count);
            versionChart.update();
        },
        error: function() {
            console.error('Failed to load version stats');
        }
    });
}

/**
 * Load source statistics for chart
 */
function loadSourceStats() {
    $.ajax({
        url: config.apiBase + '/sources?top=10',
        method: 'GET',
        success: function(data) {
            sourceChart.data.labels = data.map(d => truncateString(d.source, 25));
            sourceChart.data.datasets[0].data = data.map(d => d.count);
            sourceChart.update();
        },
        error: function() {
            console.error('Failed to load source stats');
        }
    });
}

/**
 * Load trend statistics for chart
 */
function loadTrendStats() {
    $.ajax({
        url: config.apiBase + '/trends?days=30',
        method: 'GET',
        success: function(data) {
            trendChart.data.labels = data.map(d => d.date);
            trendChart.data.datasets[0].data = data.map(d => d.count);
            trendChart.update();
        },
        error: function() {
            console.error('Failed to load trend stats');
        }
    });
}

/**
 * Show error detail in modal
 */
function showErrorDetail(id) {
    showLoading();
    $.ajax({
        url: config.apiBase + '/' + id,
        method: 'GET',
        success: function(data) {
            hideLoading();
            currentErrorId = data.id;
            
            const statusBadge = data.isResolved 
                ? '<span class="status-badge resolved"><i class="bi bi-check-circle-fill"></i> Resolved</span>'
                : '<span class="status-badge unresolved"><i class="bi bi-x-circle-fill"></i> Unresolved</span>';
            
            const timestamp = data.timestamp ? new Date(data.timestamp).toLocaleString() : 'N/A';
            
            let platformIcon = 'bi-phone';
            if (data.platform && data.platform.toLowerCase() === 'android') platformIcon = 'bi-android2';
            else if (data.platform && data.platform.toLowerCase() === 'ios') platformIcon = 'bi-apple';
            else if (data.platform && data.platform.toLowerCase() === 'windows') platformIcon = 'bi-windows';
            
            const html = `
                <div class="row">
                    <div class="col-md-6">
                        <div class="error-detail-section">
                            <h6><i class="bi bi-info-circle me-2"></i>Status</h6>
                            ${statusBadge}
                        </div>
                        <div class="error-detail-section">
                            <h6><i class="bi bi-clock me-2"></i>Timestamp</h6>
                            <p class="mb-0">${timestamp}</p>
                        </div>
                        <div class="error-detail-section">
                            <h6><i class="bi bi-code-slash me-2"></i>Source</h6>
                            <p class="mb-0">${escapeHtml(data.source || 'N/A')}</p>
                        </div>
                    </div>
                    <div class="col-md-6">
                        <div class="error-detail-section">
                            <h6><i class="${platformIcon} me-2"></i>Platform</h6>
                            <p class="mb-0">${escapeHtml(data.platform || 'N/A')}</p>
                        </div>
                        <div class="error-detail-section">
                            <h6><i class="bi bi-tag me-2"></i>App Version</h6>
                            <p class="mb-0"><span class="version-badge">${escapeHtml(data.appVersion || 'N/A')}</span></p>
                        </div>
                        <div class="error-detail-section">
                            <h6><i class="bi bi-phone me-2"></i>Device Info</h6>
                            <p class="mb-0"><span class="device-badge">${escapeHtml(data.deviceInfo || 'N/A')}</span></p>
                        </div>
                    </div>
                </div>
                <div class="error-detail-section">
                    <h6><i class="bi bi-chat-text me-2"></i>Message</h6>
                    <p class="mb-0">${escapeHtml(data.message || 'N/A')}</p>
                </div>
                <div class="error-detail-section">
                    <h6><i class="bi bi-list-ul me-2"></i>Stack Trace</h6>
                    <div class="stack-trace-container">${escapeHtml(data.stackTrace || 'No stack trace available')}</div>
                </div>
            `;
            
            $('#errorDetailContent').html(html);
            
            // Update modal buttons based on status
            if (data.isResolved) {
                $('#modalResolveBtn').hide();
                $('#modalUnresolveBtn').show();
            } else {
                $('#modalResolveBtn').show();
                $('#modalUnresolveBtn').hide();
            }
            
            $('#errorDetailModal').modal('show');
        },
        error: function() {
            hideLoading();
            toastr.error('Failed to load error details');
        }
    });
}

/**
 * Resolve a single error
 */
function resolveError(id) {
    showLoading();
    $.ajax({
        url: config.apiBase + '/' + id + '/resolve',
        method: 'PUT',
        success: function(response) {
            hideLoading();
            toastr.success(response.message || 'Error marked as resolved');
            refreshData();
            $('#errorDetailModal').modal('hide');
        },
        error: function(xhr) {
            hideLoading();
            toastr.error(xhr.responseJSON?.message || 'Failed to resolve error');
        }
    });
}

/**
 * Unresolve a single error
 */
function unresolveError(id) {
    showLoading();
    $.ajax({
        url: config.apiBase + '/' + id + '/unresolve',
        method: 'PUT',
        success: function(response) {
            hideLoading();
            toastr.success(response.message || 'Error marked as unresolved');
            refreshData();
            $('#errorDetailModal').modal('hide');
        },
        error: function(xhr) {
            hideLoading();
            toastr.error(xhr.responseJSON?.message || 'Failed to unresolve error');
        }
    });
}

/**
 * Bulk resolve multiple errors
 */
function bulkResolve(ids) {
    showLoading();
    $.ajax({
        url: config.apiBase + '/bulk-resolve',
        method: 'PUT',
        contentType: 'application/json',
        data: JSON.stringify({ ids: ids }),
        success: function(response) {
            hideLoading();
            toastr.success(response.message || `${response.affected} error(s) marked as resolved`);
            clearSelection();
            refreshData();
        },
        error: function(xhr) {
            hideLoading();
            toastr.error(xhr.responseJSON?.message || 'Failed to resolve errors');
        }
    });
}

/**
 * Bulk unresolve multiple errors
 */
function bulkUnresolve(ids) {
    showLoading();
    $.ajax({
        url: config.apiBase + '/bulk-unresolve',
        method: 'PUT',
        contentType: 'application/json',
        data: JSON.stringify({ ids: ids }),
        success: function(response) {
            hideLoading();
            toastr.success(response.message || `${response.affected} error(s) marked as unresolved`);
            clearSelection();
            refreshData();
        },
        error: function(xhr) {
            hideLoading();
            toastr.error(xhr.responseJSON?.message || 'Failed to unresolve errors');
        }
    });
}

/**
 * Reload the data table
 */
function reloadTable() {
    if (errorLogTable) {
        errorLogTable.ajax.reload(null, false);
    }
}

/**
 * Refresh all dashboard data
 */
function refreshData() {
    reloadTable();
    loadDashboardData();
}

/**
 * Update selected IDs from checkboxes
 */
function updateSelectedIds() {
    selectedIds = [];
    $('.row-checkbox:checked').each(function() {
        selectedIds.push(parseInt($(this).val()));
    });
    updateBulkActionButtons();
}

/**
 * Update bulk action button states
 */
function updateBulkActionButtons() {
    const hasSelection = selectedIds.length > 0;
    $('#bulkResolve, #bulkUnresolve').prop('disabled', !hasSelection);
}

/**
 * Clear all selections
 */
function clearSelection() {
    selectedIds = [];
    $('#selectAll').prop('checked', false);
    $('.row-checkbox').prop('checked', false);
    updateBulkActionButtons();
}

/**
 * Show confirmation dialog
 */
function showConfirmDialog(title, message, onConfirm) {
    $('#confirmModalTitle').text(title);
    $('#confirmModalMessage').text(message);
    $('#confirmModalBtn').off('click').on('click', function() {
        $('#confirmModal').modal('hide');
        onConfirm();
    });
    $('#confirmModal').modal('show');
}

/**
 * Show loading overlay
 */
function showLoading() {
    $('#loadingOverlay').removeClass('d-none');
}

/**
 * Hide loading overlay
 */
function hideLoading() {
    $('#loadingOverlay').addClass('d-none');
}

/**
 * Start auto-refresh
 */
function startAutoRefresh() {
    stopAutoRefresh();
    autoRefreshInterval = setInterval(function() {
        refreshData();
    }, config.autoRefreshIntervalMs);
}

/**
 * Stop auto-refresh
 */
function stopAutoRefresh() {
    if (autoRefreshInterval) {
        clearInterval(autoRefreshInterval);
        autoRefreshInterval = null;
    }
}

/**
 * Update chart theme for dark mode
 */
function updateChartTheme() {
    const isDark = $('body').hasClass('dark-mode');
    const textColor = isDark ? '#e9ecef' : '#212529';
    const gridColor = isDark ? 'rgba(255, 255, 255, 0.1)' : 'rgba(0, 0, 0, 0.1)';
    
    const charts = [platformChart, resolutionChart, versionChart, sourceChart, trendChart];
    
    charts.forEach(chart => {
        if (chart) {
            if (chart.options.plugins && chart.options.plugins.legend) {
                chart.options.plugins.legend.labels.color = textColor;
            }
            if (chart.options.scales) {
                Object.keys(chart.options.scales).forEach(key => {
                    chart.options.scales[key].ticks = chart.options.scales[key].ticks || {};
                    chart.options.scales[key].ticks.color = textColor;
                    chart.options.scales[key].grid = chart.options.scales[key].grid || {};
                    chart.options.scales[key].grid.color = gridColor;
                });
            }
            chart.update();
        }
    });
}

/**
 * Escape HTML to prevent XSS (efficient implementation with lookup table)
 */
const htmlEscapeMap = {
    '&': '&amp;',
    '<': '&lt;',
    '>': '&gt;',
    '"': '&quot;',
    "'": '&#39;'
};

function escapeHtml(text) {
    if (!text) return '';
    return String(text).replace(/[&<>"']/g, function(char) {
        return htmlEscapeMap[char];
    });
}

/**
 * Truncate string with ellipsis
 */
function truncateString(str, maxLength) {
    if (!str) return '';
    return str.length > maxLength ? str.substring(0, maxLength) + '...' : str;
}
