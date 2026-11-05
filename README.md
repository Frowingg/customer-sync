# CustomerSync

An integration service that pulls customer records from a **legacy SOAP/WCF service**, validates them, synchronizes them into **SQL Server**, and reports every data discrepancy through a REST API.

The domain and all data are invented. The project exists to demonstrate a typical enterprise integration scenario: a legacy system you cannot change, a modern .NET service that consumes it, and reliable reporting on data quality.

> **Status:** work in progress. See [docs/ROADMAP.md](docs/ROADMAP.md).

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

- Sync API (Swagger): http://localhost:5000/swagger
- Legacy SOAP service (WSDL): http://localhost:5001/CustomerService.svc?wsdl

Trigger a sync:

```bash
curl -X POST http://localhost:5000/api/sync/runs
```

## Develop and test

Requirements: .NET 10 SDK.

```bash
dotnet build
dotnet test
```

## Tech

C#, .NET 10, ASP.NET Core Web API, CoreWCF, Entity Framework Core, SQL Server, xUnit, Docker Compose, GitHub Actions.
