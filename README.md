# Medicare Claims Manager

ASP.NET Core MVC application for managing Medicare claim workflows, built with a professional healthcare operations interface.

## Stack

- ASP.NET Core MVC on .NET 9
- Razor Pages for ASP.NET Core Identity
- Bootstrap 5, HTML5, CSS3, and JavaScript
- C#, Entity Framework Core, and LINQ
- SQL Server locally, Azure SQL Database for cloud deployment
- ASP.NET Core Identity with role-based access control

## Theme

| Element | Color |
| --- | --- |
| Sidebar | `#0B3C5D` |
| Primary | `#1E88E5` |
| Secondary | `#E3F2FD` |
| Success | `#2E7D32` |
| Warning | `#F9A825` |
| Error | `#C62828` |
| Background | `#F5F7FA` |
| Cards | `#FFFFFF` |
| Borders | `#D6E4F0` |
| Text | `#263238` |

## Roadmap

1. Project Foundation
2. Authentication and Identity
3. Dashboard
4. Patients Module
5. Providers Module
6. Claims Module
7. Reports and Analytics
8. Admin Panel
9. Deployment and Documentation

## Getting Started

Restore and build:

```bash
dotnet restore
dotnet build
```

Apply the database migrations:

```bash
dotnet ef database update
```

Run the app:

```bash
dotnet run
```

The default development connection string uses SQL Server LocalDB:

```text
Server=(localdb)\mssqllocaldb;Database=MedicareClaimsManager_Dev;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True
```

Override `ConnectionStrings:DefaultConnection` for Azure SQL or another SQL Server instance with user secrets or environment variables.

## Security Notes

- Do not commit real patient data, Medicare identifiers, credentials, API keys, or production exports.
- Use environment variables, user secrets, or a secrets manager for deployed configuration.
- Keep sample data synthetic and clearly labeled.
- Keep role assignments intentional: `Administrator`, `ClaimsManager`, `BillingSpecialist`, and `ReadOnly` are seeded at startup after the database exists.
