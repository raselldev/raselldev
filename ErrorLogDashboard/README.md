# SQL Server Error Log Dashboard

A comprehensive, interactive dashboard for monitoring error logs from SQL Server. Built with ASP.NET MVC + Web API (.NET Framework 4.8), this dashboard helps users understand error patterns at a glance, with the ability to mark errors as **Resolved** or **Unresolved**.

![Dashboard Preview](docs/dashboard-preview.png)

## Features

### Summary Cards
- **Total Errors Count** - Overall count of all error logs
- **Unresolved Errors** - Highlighted in red for immediate attention
- **Resolved Errors** - Highlighted in green showing addressed issues
- **Unique Error Sources** - Count of distinct error sources
- **Affected Platforms** - Number of platforms with errors

### Interactive Charts
- **Pie Chart**: Error distribution by Platform (Android, iOS, Windows, etc.)
- **Pie Chart**: Resolved vs Unresolved errors ratio
- **Bar Chart**: Errors grouped by App Version
- **Bar Chart**: Top 10 Error Sources
- **Line/Area Chart**: Error trends over the last 30 days

### Data Table Features
- Server-side pagination (10, 25, 50, 100 per page)
- Real-time search functionality
- Sortable columns
- Filter by:
  - Platform (dropdown)
  - App Version (dropdown)
  - Source (dropdown)
  - Resolution Status (All/Resolved/Unresolved)
  - Date Range
- Status indicator column with color-coded badges
- Action buttons per row for quick resolution management

### Resolution Management
- **Single Item**: Click button to toggle resolution status
- **Bulk Actions**: Select multiple items with checkboxes, then resolve/unresolve all at once
- Confirmation dialog for bulk actions
- Success/Error toast notifications
- Real-time UI update without page refresh (AJAX)

### Error Detail Modal
- Timestamp (formatted)
- Full error message
- Complete stack trace (syntax highlighted, scrollable)
- Source, App Version, Platform, Device Info
- Resolution Status with toggle buttons

### UI/UX Features
- Clean, modern design using Bootstrap 5
- Responsive layout (mobile-friendly)
- Color-coded status indicators (🔴 Red for unresolved, 🟢 Green for resolved)
- Loading spinners for async operations
- Toast notifications for actions
- Auto-refresh option (every 30 seconds)
- Dark/Light mode toggle

## Prerequisites

- **.NET Framework 4.8** - [Download](https://dotnet.microsoft.com/download/dotnet-framework/net48)
- **SQL Server 2016+** - For database storage
- **IIS 10+** - For deployment (or IIS Express for development)
- **Visual Studio 2019/2022** - Recommended for development

## Installation

### 1. Clone the Repository

```bash
git clone https://github.com/your-org/ErrorLogDashboard.git
cd ErrorLogDashboard
```

### 2. Database Setup

Create the required database objects in your SQL Server:

#### Create the Error Log Table

```sql
CREATE TABLE HD_ERROR_LOG_V2 (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Timestamp DATETIME NOT NULL DEFAULT GETDATE(),
    Message NVARCHAR(MAX),
    StackTrace NVARCHAR(MAX),
    Source NVARCHAR(500),
    AppVersion NVARCHAR(50),
    Platform NVARCHAR(50),
    DeviceInfo NVARCHAR(500),
    IsResolved BIT NOT NULL DEFAULT 0
);

-- Create indexes for better performance
CREATE INDEX IX_HD_ERROR_LOG_V2_Timestamp ON HD_ERROR_LOG_V2(Timestamp DESC);
CREATE INDEX IX_HD_ERROR_LOG_V2_IsResolved ON HD_ERROR_LOG_V2(IsResolved);
CREATE INDEX IX_HD_ERROR_LOG_V2_Platform ON HD_ERROR_LOG_V2(Platform);
CREATE INDEX IX_HD_ERROR_LOG_V2_Source ON HD_ERROR_LOG_V2(Source);
```

#### (Optional) Create the View for Reading

```sql
CREATE VIEW VIEW_ERROR_LOG AS
SELECT 
    Message, 
    StackTrace, 
    Source, 
    AppVersion, 
    Platform, 
    DeviceInfo, 
    COUNT(*) AS TOTAL_ERROR
FROM HD_ERROR_LOG_V2
GROUP BY Message, StackTrace, Source, AppVersion, Platform, DeviceInfo;
```

#### Insert Sample Data (for testing)

```sql
INSERT INTO HD_ERROR_LOG_V2 (Timestamp, Message, StackTrace, Source, AppVersion, Platform, DeviceInfo, IsResolved)
VALUES 
(DATEADD(HOUR, -1, GETDATE()), 'A Task''s exception(s) were not observed either by Waiting on the Task or accessing its Exception property.', '[Exception Level 0] System.AggregateException: A Task''s exception(s) were not observed...', 'TaskScheduler.UnobservedTaskException', '2.0.0', 'Android', 'realme RMX1921', 0),
(DATEADD(HOUR, -2, GETDATE()), 'Network connection timeout', 'System.Net.WebException: The operation has timed out...', 'HttpClient.SendAsync', '2.0.0', 'iOS', 'iPhone 14', 0),
(DATEADD(HOUR, -3, GETDATE()), 'Object reference not set to an instance of an object', 'System.NullReferenceException at MyApp.Services.UserService.GetUser()...', 'UserService.GetUser', '1.9.5', 'Android', 'Samsung Galaxy S21', 1),
(DATEADD(DAY, -1, GETDATE()), 'Database connection failed', 'System.Data.SqlClient.SqlException: A network-related or instance-specific error...', 'DatabaseService.Connect', '2.0.0', 'Windows', 'Windows 11', 0);
```

### 3. Configure Connection String

Edit `ErrorLogDashboard.Web/Web.config` and update the connection string:

```xml
<connectionStrings>
  <add name="ErrorLogDb" 
       connectionString="Server=YOUR_SERVER;Database=YOUR_DB;Integrated Security=True;" 
       providerName="System.Data.SqlClient" />
</connectionStrings>
```

**Connection String Examples:**

```xml
<!-- Windows Authentication -->
<add name="ErrorLogDb" 
     connectionString="Server=localhost;Database=ErrorLogDB;Integrated Security=True;" 
     providerName="System.Data.SqlClient" />

<!-- SQL Server Authentication -->
<add name="ErrorLogDb" 
     connectionString="Server=myserver.database.windows.net;Database=ErrorLogDB;User Id=myuser;Password=mypassword;" 
     providerName="System.Data.SqlClient" />
```

### 4. Restore NuGet Packages

Open the solution in Visual Studio and restore packages, or run:

```bash
nuget restore ErrorLogDashboard.sln
```

### 5. Build and Run

```bash
# Build the solution
msbuild ErrorLogDashboard.sln /p:Configuration=Release

# Or use Visual Studio: Build > Build Solution (Ctrl+Shift+B)
```

Run with IIS Express (F5 in Visual Studio) or deploy to IIS.

## Project Structure

```
ErrorLogDashboard/
├── ErrorLogDashboard.sln              # Visual Studio Solution
├── ErrorLogDashboard.Web/             # Web Application Project
│   ├── Controllers/
│   │   ├── HomeController.cs          # MVC Controller for views
│   │   └── Api/
│   │       └── ErrorLogApiController.cs  # Web API Controller
│   ├── Models/
│   │   ├── ErrorLog.cs                # Error log entity models
│   │   ├── ErrorLogSummary.cs         # Summary and statistics models
│   │   └── ErrorLogFilter.cs          # Filter parameters model
│   ├── Services/
│   │   ├── IErrorLogService.cs        # Service interface
│   │   └── ErrorLogService.cs         # Service implementation (Dapper)
│   ├── ViewModels/
│   │   └── DashboardViewModel.cs      # Dashboard view model
│   ├── Views/
│   │   ├── Shared/
│   │   │   └── _Layout.cshtml         # Master layout page
│   │   └── Home/
│   │       └── Index.cshtml           # Dashboard view
│   ├── Scripts/
│   │   └── dashboard.js               # Dashboard JavaScript
│   ├── Content/
│   │   └── dashboard.css              # Custom styles
│   ├── App_Start/
│   │   ├── WebApiConfig.cs            # Web API configuration
│   │   ├── RouteConfig.cs             # MVC routing configuration
│   │   └── BundleConfig.cs            # Bundle configuration
│   ├── Global.asax                    # Application entry point
│   ├── Global.asax.cs
│   ├── packages.config                # NuGet packages
│   └── Web.config                     # Application configuration
└── README.md                          # This file
```

## API Documentation

### Read Operations

#### Get All Errors (with filters)
```http
GET /api/errorlog?platform=Android&isResolved=false&page=1&pageSize=10
```

**Query Parameters:**
| Parameter | Type | Description |
|-----------|------|-------------|
| platform | string | Filter by platform |
| appVersion | string | Filter by app version |
| source | string | Filter by error source |
| isResolved | boolean | Filter by resolution status |
| startDate | datetime | Filter by start date |
| endDate | datetime | Filter by end date |
| search | string | Search in message/stacktrace/source |
| page | int | Page number (default: 1) |
| pageSize | int | Items per page (default: 10, max: 100) |
| sortColumn | string | Column to sort by |
| sortDirection | string | ASC or DESC |

**Response:**
```json
{
  "items": [...],
  "totalCount": 100,
  "page": 1,
  "pageSize": 10,
  "totalPages": 10,
  "hasPreviousPage": false,
  "hasNextPage": true
}
```

#### Get Single Error
```http
GET /api/errorlog/{id}
```

#### Get Dashboard Summary
```http
GET /api/errorlog/summary
```

**Response:**
```json
{
  "totalErrors": 150,
  "unresolvedErrors": 45,
  "resolvedErrors": 105,
  "uniqueErrorSources": 12,
  "affectedPlatforms": 3
}
```

#### Get Platform Statistics
```http
GET /api/errorlog/platforms
```

#### Get Version Statistics
```http
GET /api/errorlog/versions
```

#### Get Top Error Sources
```http
GET /api/errorlog/sources?top=10
```

#### Get Error Trends
```http
GET /api/errorlog/trends?days=30
```

#### Get Resolution Statistics
```http
GET /api/errorlog/resolution-stats
```

### Write Operations

#### Mark Error as Resolved
```http
PUT /api/errorlog/{id}/resolve
```

#### Mark Error as Unresolved
```http
PUT /api/errorlog/{id}/unresolve
```

#### Bulk Resolve
```http
PUT /api/errorlog/bulk-resolve
Content-Type: application/json

{
  "ids": [1, 2, 3, 4, 5]
}
```

#### Bulk Unresolve
```http
PUT /api/errorlog/bulk-unresolve
Content-Type: application/json

{
  "ids": [1, 2, 3, 4, 5]
}
```

## IIS Deployment Guide

### 1. Publish the Application

In Visual Studio:
1. Right-click the project > **Publish**
2. Choose **Folder** as target
3. Select publish folder
4. Click **Publish**

### 2. Configure IIS

1. Open **IIS Manager**
2. Create a new **Application Pool**:
   - Name: `ErrorLogDashboard`
   - .NET CLR Version: `.NET CLR Version v4.0.30319`
   - Managed Pipeline Mode: `Integrated`
3. Create a new **Website** or **Application**:
   - Physical Path: Point to published folder
   - Application Pool: Select `ErrorLogDashboard`
   - Binding: Configure host name and port

### 3. Set Permissions

Ensure the IIS application pool identity has:
- **Read** access to the application folder
- **SQL Server access** (if using Windows Authentication)

### 4. Configure Application Pool Identity

For SQL Server Windows Authentication:
1. Open Application Pool > Advanced Settings
2. Set **Identity** to a domain account with database access
3. Or configure SQL Server to allow the default `IIS AppPool\ErrorLogDashboard` identity

## Troubleshooting

### Common Issues

**1. Database Connection Error**
- Verify connection string in Web.config
- Check SQL Server is running and accessible
- Verify database user permissions

**2. 404 Not Found for API**
- Ensure Web API is registered in Global.asax
- Check IIS has runAllManagedModulesForAllRequests enabled

**3. Charts Not Loading**
- Check browser console for JavaScript errors
- Verify API endpoints are returning data

**4. Build Errors**
- Run `nuget restore` to restore packages
- Ensure .NET Framework 4.8 is installed

## Technology Stack

- **Backend**: ASP.NET MVC 5 + Web API 2 (.NET Framework 4.8)
- **Frontend**: HTML5, CSS3, JavaScript (ES6+)
- **CSS Framework**: Bootstrap 5.3
- **Charts**: Chart.js 4.4
- **Data Table**: DataTables 1.13
- **Notifications**: Toastr
- **Date Picker**: Flatpickr
- **Database Access**: Dapper ORM
- **Database**: SQL Server

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Contributing

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## Support

For support, please open an issue in the GitHub repository or contact the development team.
