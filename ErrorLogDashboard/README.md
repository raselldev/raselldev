# Error Log Dashboard

A comprehensive, interactive dashboard for monitoring error logs from SQL Server. Built with ASP.NET Web API (.NET Framework 4.8) and modern frontend technologies.

<!-- Screenshot placeholder: Add dashboard-overview.png to docs/screenshots/ after deployment -->

## Features

### Summary Cards
- **Total Errors Count**: Shows the sum of all error occurrences
- **Unique Error Sources**: Number of distinct error sources
- **Affected Platforms**: Count of platforms with errors (Android, iOS, Windows, etc.)
- **Most Affected App Version**: The version with the highest error count

### Interactive Charts
- **Pie Chart**: Error distribution by Platform
- **Bar Chart**: Errors by App Version
- **Horizontal Bar Chart**: Top 10 Error Sources

### Data Table
- Searchable and sortable columns
- Pagination with configurable page size
- Filter by Platform, App Version, and Source
- Click on any row to see full error details

### Error Detail Modal
When clicking on an error row, a modal displays:
- Full error message
- Complete stack trace (formatted and readable)
- Source, App Version, Platform
- Device Info
- Total occurrences

### Additional Features
- Export to CSV/Excel
- Auto-refresh option (30-second interval)
- Responsive design for all screen sizes
- Color-coded severity indicators

## Tech Stack

- **Backend**: ASP.NET Web API (.NET Framework 4.8)
- **Frontend**: HTML5/CSS3/JavaScript
- **CSS Framework**: Bootstrap 5.3
- **Charts**: Chart.js 4.4
- **Icons**: Bootstrap Icons
- **Database**: SQL Server

## Prerequisites

- Windows Server 2016+ or Windows 10+
- .NET Framework 4.8 Runtime
- SQL Server 2016+ (or SQL Server Express)
- IIS 10+ (for deployment) or Visual Studio 2019+ (for development)

## Project Structure

```
ErrorLogDashboard/
├── ErrorLogDashboard.sln              # Solution file
├── ErrorLogDashboard.Web/
│   ├── App_Start/
│   │   ├── WebApiConfig.cs            # Web API routing and configuration
│   │   └── RouteConfig.cs             # MVC routing configuration
│   ├── Controllers/
│   │   ├── ErrorLogController.cs      # API endpoints for error data
│   │   └── HomeController.cs          # MVC controller for dashboard view
│   ├── Models/
│   │   └── ErrorLog.cs                # Data models and DTOs
│   ├── Services/
│   │   └── ErrorLogService.cs         # Database access layer
│   ├── Views/
│   │   ├── Home/
│   │   │   └── Index.cshtml           # Main dashboard view
│   │   ├── Shared/
│   │   │   └── _Layout.cshtml         # Layout template
│   │   ├── _ViewStart.cshtml
│   │   └── web.config
│   ├── Scripts/
│   │   └── dashboard.js               # Frontend JavaScript
│   ├── Content/
│   │   └── dashboard.css              # Custom styles
│   ├── Properties/
│   │   └── AssemblyInfo.cs
│   ├── Global.asax
│   ├── Global.asax.cs
│   ├── Web.config                     # Application configuration
│   └── packages.config                # NuGet packages
└── README.md
```

## Installation

### 1. Clone or Download

```bash
git clone https://github.com/yourusername/ErrorLogDashboard.git
cd ErrorLogDashboard
```

### 2. Configure Database Connection

Edit `ErrorLogDashboard.Web/Web.config` and update the connection string:

```xml
<connectionStrings>
  <add name="ErrorLogDb" 
       connectionString="Server=YOUR_SERVER;Database=YOUR_DB;Integrated Security=True;" 
       providerName="System.Data.SqlClient"/>
</connectionStrings>
```

**Connection String Examples:**

Windows Authentication:
```
Server=localhost\SQLEXPRESS;Database=ErrorLogDB;Integrated Security=True;
```

SQL Server Authentication:
```
Server=192.168.1.100;Database=ErrorLogDB;User Id=myuser;Password=mypassword;
```

### 3. Create the Database View

Ensure your SQL Server database has the `VIEW_ERROR_LOG` view:

```sql
CREATE VIEW VIEW_ERROR_LOG AS
SELECT 
    message,
    StackTrace,
    source,
    AppVersion,
    Platform,
    DeviceInfo,
    COUNT(*) as TOTAL_ERROR
FROM YourErrorLogTable
GROUP BY message, StackTrace, source, AppVersion, Platform, DeviceInfo
```

Or if you already have aggregated data, adjust the view accordingly.

### 4. Restore NuGet Packages

Using Visual Studio:
1. Open `ErrorLogDashboard.sln`
2. Right-click the solution → "Restore NuGet Packages"

Using Package Manager Console:
```powershell
nuget restore ErrorLogDashboard.sln
```

### 5. Build and Run

**Visual Studio:**
1. Press F5 or click "Start"
2. The dashboard will open in your default browser

**Command Line:**
```bash
msbuild ErrorLogDashboard.sln /p:Configuration=Release
```

## Deployment (IIS)

### 1. Publish the Application

In Visual Studio:
1. Right-click the web project → "Publish"
2. Choose "Folder" as the target
3. Publish to a local folder

### 2. Configure IIS

1. Open IIS Manager
2. Create a new Application Pool:
   - Name: `ErrorLogDashboard`
   - .NET CLR Version: `v4.0`
   - Managed Pipeline Mode: `Integrated`

3. Create a new Website or Application:
   - Physical path: Published folder location
   - Application pool: `ErrorLogDashboard`
   - Binding: Configure as needed (port, hostname)

4. Set folder permissions:
   - Grant `IIS_IUSRS` read permissions to the application folder

### 3. Test the Deployment

Navigate to your configured URL (e.g., `http://localhost/ErrorLogDashboard`)

## API Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/errorlog` | Get paginated error logs with optional filters |
| GET | `/api/errorlog/summary` | Get dashboard summary statistics |
| GET | `/api/errorlog/platforms` | Get error count grouped by platform |
| GET | `/api/errorlog/versions` | Get error count grouped by app version |
| GET | `/api/errorlog/sources` | Get top 10 error sources |
| GET | `/api/errorlog/{id}` | Get specific error details |
| GET | `/api/errorlog/filters/{column}` | Get distinct values for filter dropdowns |

### Query Parameters for `/api/errorlog`

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| platform | string | null | Filter by platform |
| appVersion | string | null | Filter by app version |
| source | string | null | Filter by error source |
| searchTerm | string | null | Search in message, stacktrace, source |
| page | int | 1 | Page number |
| pageSize | int | 10 | Results per page (max 100) |
| sortBy | string | TotalError | Sort field |
| sortDescending | bool | true | Sort direction |

### Example API Requests

```bash
# Get all errors (first page)
GET /api/errorlog

# Get Android errors
GET /api/errorlog?platform=Android

# Search errors
GET /api/errorlog?searchTerm=NullReferenceException

# Get summary
GET /api/errorlog/summary
```

## Configuration Options

### Auto-Refresh Interval

Modify `AUTO_REFRESH_INTERVAL` in `Scripts/dashboard.js`:

```javascript
const AUTO_REFRESH_INTERVAL = 30000; // milliseconds
```

### CORS Configuration

CORS is enabled for all origins by default. For production, update `App_Start/WebApiConfig.cs`:

```csharp
var cors = new EnableCorsAttribute("https://yourdomain.com", "*", "*");
config.EnableCors(cors);
```

## Screenshots

*Note: Screenshots will be added after the dashboard is deployed and running.*

### Dashboard Overview
*Shows summary cards, charts, and data table*
<!-- Add screenshot: docs/screenshots/dashboard-overview.png -->

### Error Detail Modal
*Detailed view of a specific error*
<!-- Add screenshot: docs/screenshots/error-detail.png -->

### Filtered Results
*Dashboard with applied filters*
<!-- Add screenshot: docs/screenshots/filtered-results.png -->

## Database Schema

The dashboard queries from `VIEW_ERROR_LOG` with the following columns:

| Column | Type | Description |
|--------|------|-------------|
| message | nvarchar | Error message text |
| StackTrace | nvarchar(max) | Full stack trace |
| source | nvarchar | Error source (e.g., method name) |
| AppVersion | nvarchar | Application version |
| Platform | nvarchar | Platform (Android, iOS, Windows) |
| DeviceInfo | nvarchar | Device model/info |
| TOTAL_ERROR | int | Count of error occurrences |

## Troubleshooting

### Common Issues

**Connection String Error:**
- Verify SQL Server is running
- Check firewall settings
- Ensure the database user has SELECT permissions on VIEW_ERROR_LOG

**404 Error on API:**
- Ensure WebApiConfig.Register is called in Global.asax.cs
- Check that the routing is correctly configured

**Charts Not Loading:**
- Verify Chart.js CDN is accessible
- Check browser console for JavaScript errors

**CORS Errors:**
- Update WebApiConfig.cs with appropriate origins
- Ensure the Microsoft.AspNet.WebApi.Cors package is installed

## License

This project is provided as-is for educational and internal use purposes.

## Contributing

1. Fork the repository
2. Create a feature branch
3. Submit a pull request

## Support

For issues and questions, please create an issue in the GitHub repository.
