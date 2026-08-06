# Architecture Overview

Medicare Claims Manager is an ASP.NET Core MVC application using Entity Framework Core and ASP.NET Core Identity.

## Main Areas

- Dashboard: operational summary of patient, provider, and claim activity.
- Patients: CRUD module for patient demographic records.
- Providers: CRUD module for provider organizations and NPI data.
- Claims: intake CRUD, controlled status workflow, and audit history.
- Reports: analytics across claim status, priority, financials, provider performance, and workflow activity.
- Admin: administrator-only user role management.

## Data Model

Core entities:

- `Patient`
- `Provider`
- `Claim`
- `ClaimAuditEntry`

Identity entities are provided by ASP.NET Core Identity.

## Claim Workflow

Allowed transitions:

| From | To |
| --- | --- |
| `Draft` | `Submitted` |
| `Submitted` | `InReview`, `PendingDocumentation` |
| `InReview` | `PendingDocumentation`, `Approved`, `Denied` |
| `PendingDocumentation` | `InReview`, `Denied` |
| `Approved` | `Paid` |
| `Denied` | Terminal |
| `Paid` | Terminal |

Status changes are performed from Claim Details and recorded in `ClaimAuditEntry`.
