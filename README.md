# CustomerSync
![CI](https://github.com/Frowingg/customer-sync/actions/workflows/ci.yml/badge.svg)

An integration service that pulls customer records from a **legacy SOAP/WCF service**, validates them, synchronizes them into **SQL Server**, and reports every data discrepancy through a REST API.

The domain and all data are invented. The project exists to demonstrate a typical enterprise integration scenario: a legacy system you cannot change, a modern .NET service that consumes it, and reliable reporting on data quality.

> **Status:** complete as a demo. Possible next steps are listed in docs/ROADMAP.md.

## What it does

- **Legacy side** (`Legacy.CustomerService`): a SOAP service built with [CoreWCF](https://github.com/CoreWCF/CoreWCF) on .NET 10, exposing customer data with a few deliberate anomalies (invalid emails, duplicates, missing fields).
- **Modern side** (`CustomerSync.Api`): an ASP.NET Core Web API that reads the legacy service, validates each record, inserts or updates customers in SQL Server (Entity Framework Core), and flags problems instead of silently dropping them.
- **Reporting:** every sync run is stored with counters (inserted, updated, unchanged) and a list of discrepancies, available over REST.

## Architecture

See [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md) for the diagram, sync rules and design decisions.

## Run it

Requirements: Docker Desktop (or Docker Engine + Compose).

```bash
cp .env.example .env      # set a strong SQL Server password
docker compose up --build
```

- Sync API: http://localhost:5000/api/sync/runs (health check: http://localhost:5000/health)
- Legacy SOAP service endpoint: http://localhost:5001/CustomerService.svc

Trigger a sync:

```bash
curl -X POST http://localhost:5000/api/sync/runs
```

First run against the seeded legacy data (30 records, 5 of them deliberately dirty):

```json
{ "id": 1, "inserted": 26, "updated": 0, "unchanged": 0, "discrepancyCount": 5 }
```

`GET /api/sync/runs/1/discrepancies`:

```json
[
  { "type": "InvalidRecord",     "legacyId": 7,  "detail": "Email is missing or malformed." },
  { "type": "InvalidRecord",     "legacyId": 12, "detail": "Name is required." },
  { "type": "InvalidRecord",     "legacyId": 15, "detail": "Tax code must be 16 alphanumeric characters or 11 digits." },
  { "type": "InvalidRecord",     "legacyId": 25, "detail": "Tax code is required." },
  { "type": "DuplicateInSource", "legacyId": 4,  "detail": "LegacyId returned more than once; the first occurrence was kept." }
]
```

Running the sync again changes nothing (`inserted: 0`, `unchanged: 26`): the sync is idempotent.

## Develop and test

Requirements: .NET 10 SDK.

```bash
dotnet build
dotnet test
```

Test coverage: **83.6% lines · 80.4% branches** (EF Core migrations excluded). To reproduce:

```bash
dotnet test --collect:"XPlat Code Coverage" --settings coverlet.runsettings
```

## Tech

C#, .NET 10, ASP.NET Core Web API, CoreWCF, Entity Framework Core, SQL Server, xUnit, Docker Compose, GitHub Actions.
