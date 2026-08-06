# Medicare Claims Manager

ASP.NET Core MVC application for managing Medicare claim workflows, built with a professional healthcare operations interface.

## Portfolio Summary

Medicare Claims Manager demonstrates C#, ASP.NET Core MVC, Entity Framework Core, SQL Server, Identity, role-based access control, operational reporting, audit trails, automated tests, and CI/CD deployment readiness in a healthcare revenue cycle scenario.

## Features

- Secure authentication with ASP.NET Core Identity.
- Role-based access model for `Administrator`, `ClaimsManager`, `BillingSpecialist`, and `ReadOnly`.
- Patients CRUD with duplicate Medicare number validation.
- Providers CRUD with NPI validation.
- Claims intake CRUD with patient/provider selection.
- Controlled claim status workflow with terminal states.
- Claim audit trail for creation, field edits, and status changes.
- Reports dashboard for status mix, priority mix, provider performance, financial totals, and workflow activity.
- Admin panel for user role assignments.
- Development-only synthetic seed data for demo-ready first runs.
- Automated xUnit tests and GitHub Actions CI.
- Azure App Service deployment workflow template.

## Application Screens

Capture these screens after running migrations and launching the app with development seed data enabled:

- Dashboard: operational metrics, recent workflow, and recent claim activity.
- Claims: work queue with status filtering.
- Claim Details: workflow transition controls and audit trail.
- Reports: financial summary, status breakdown, priority mix, and provider performance.
- Admin: user access and role assignment.

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

Completed:

- Project foundation
- Authentication and Identity
- Dashboard
- Patients module
- Providers module
- Claims module
- Claim workflow and audit trail
- Reports and analytics
- Admin panel
- Deployment documentation
- Development seed data
- Automated tests and CI/CD workflow templates

Next improvements:

- End-to-end browser tests
- Production Azure resource provisioning scripts
- Portfolio screenshots after live deployment

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

To promote the first registered user to `Administrator`, configure:

```text
AdminUser:Email=admin@example.com
```

The user must already exist in ASP.NET Core Identity. The role is assigned during application startup.

Development runs seed synthetic demo data by default when the patient, provider, and claim tables are empty:

```text
SeedData:Enabled=true
```

Set it to `false` to start with an empty local database.

## Documentation

- [Architecture Overview](docs/ARCHITECTURE.md)
- [Deployment Guide](docs/DEPLOYMENT.md)
- [Operations Guide](docs/OPERATIONS.md)
- [Portfolio Notes](docs/PORTFOLIO.md)

## Quality Checks

Run the full local verification suite:

```bash
dotnet restore medicare-claims-manager.csproj
dotnet restore tests/MedicareClaimsManager.Tests/MedicareClaimsManager.Tests.csproj
dotnet build medicare-claims-manager.csproj --configuration Release --no-restore
dotnet test tests/MedicareClaimsManager.Tests/MedicareClaimsManager.Tests.csproj --configuration Release --no-restore
dotnet publish medicare-claims-manager.csproj --configuration Release --output ./publish-check
```

GitHub Actions runs the same restore, build, test, and publish validation flow on pushes and pull requests to `main`.

## Deployment

The project targets Azure App Service with Azure SQL Database. See [Deployment Guide](docs/DEPLOYMENT.md).

Live Azure App Service:

- https://medicare-claims-manager-75759.azurewebsites.net

Deployment workflow:

- `.github/workflows/ci.yml`: restore, build, test, publish validation.
- `.github/workflows/azure-deploy.yml`: publish and deploy to Azure App Service after CI succeeds on `main`.

## Security Notes

- Do not commit real patient data, Medicare identifiers, credentials, API keys, or production exports.
- Use environment variables, user secrets, or a secrets manager for deployed configuration.
- Keep sample data synthetic and clearly labeled.
- Keep role assignments intentional: `Administrator`, `ClaimsManager`, `BillingSpecialist`, and `ReadOnly` are seeded at startup after the database exists.
