# Operations Guide

## First Administrator

Roles are seeded at startup:

- `Administrator`
- `ClaimsManager`
- `BillingSpecialist`
- `ReadOnly`

To promote a first administrator, register the user through the app, then configure:

```text
AdminUser__Email=<registered-user-email>
```

Restart the app. The user will be added to the `Administrator` role during startup.

## Database Updates

Create a migration after model changes:

```bash
dotnet ef migrations add <MigrationName>
```

Review generated migration files before committing. Apply migrations:

```bash
dotnet ef database update
```

## Sensitive Data

- Do not use real patient records for screenshots, demos, or tests.
- Do not place Medicare numbers, credentials, exports, or raw claim files in the repository.
- Keep operational exports outside source control.

## Common Checks

```bash
dotnet restore
dotnet build
dotnet test
```

The GitHub Actions CI workflow runs restore, Release build, tests, and publish validation on pushes and pull requests to `main`.
