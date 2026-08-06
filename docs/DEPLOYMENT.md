# Deployment Guide

This project is designed for Azure App Service with Azure SQL Database. It can also run against any reachable SQL Server instance.

## Prerequisites

- .NET 9 SDK locally or in CI.
- SQL Server LocalDB for local development, or another SQL Server instance.
- Azure App Service configured for .NET 9.
- Azure SQL Database with firewall/network access configured for the app.

## Required Configuration

Use environment variables or App Service application settings. Do not commit production secrets.

| Setting | Purpose |
| --- | --- |
| `ASPNETCORE_ENVIRONMENT` | Use `Production` in deployed environments. |
| `ConnectionStrings__DefaultConnection` | SQL Server or Azure SQL connection string. |
| `AdminUser__Email` | Existing Identity user to promote to `Administrator` at startup. |

Azure SQL connection string shape:

```text
Server=tcp:<server>.database.windows.net,1433;Initial Catalog=<database>;Persist Security Info=False;User ID=<user>;Password=<password>;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;
```

## Local Release Build

```bash
dotnet restore
dotnet build --configuration Release
dotnet publish --configuration Release --output ./publish
```

## Database Migrations

Apply migrations before sending production traffic to a new version:

```bash
dotnet ef database update --connection "<production-connection-string>"
```

For Azure App Service, run migrations from a secure operator machine or CI job with access to Azure SQL. Avoid running migrations automatically on every web app startup in production.

## Azure App Service Steps

1. Create an Azure SQL Database.
2. Create an Azure App Service using the .NET 9 runtime.
3. Add `ConnectionStrings__DefaultConnection` as an App Service application setting.
4. Add `AdminUser__Email` after the first admin account is registered.
5. Publish the app from CI or local `dotnet publish` output.
6. Run EF migrations against Azure SQL.
7. Restart the App Service.
8. Register the first admin user, then restart once more if `AdminUser__Email` was added after registration.

## GitHub Actions Deployment

The repository includes `.github/workflows/azure-deploy.yml`.

Configure these repository secrets:

| Secret | Purpose |
| --- | --- |
| `AZURE_WEBAPP_NAME` | Azure App Service name. |
| `AZURE_WEBAPP_PUBLISH_PROFILE` | Publish profile XML from Azure App Service. |

The deployment workflow can be run manually with `workflow_dispatch`. It also runs after the `CI` workflow succeeds on `main`.

## Current Azure Deployment

| Resource | Value |
| --- | --- |
| Resource group | `rg-medicare-claims-manager` |
| Region | `Central US` |
| App Service | `medicare-claims-manager-75759` |
| App URL | `https://medicare-claims-manager-75759.azurewebsites.net` |
| SQL server | `mcm-sql-75759.database.windows.net` |
| SQL database | `MedicareClaimsManagerDb` |

## Deployment Checklist

- Production connection string is stored in App Service settings, not source code.
- `ASPNETCORE_ENVIRONMENT` is set to `Production`.
- Database migrations have been applied successfully.
- First administrator has been promoted through `AdminUser__Email`.
- Real PHI is not present in source, seed data, logs, or exports.
- App Service HTTPS-only is enabled.
- Azure SQL firewall/network rules allow only required access.
- Logs are configured for operational monitoring.
