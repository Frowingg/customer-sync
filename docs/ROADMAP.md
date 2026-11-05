# Roadmap

- [ ] 1. Solution skeleton, Docker Compose, CI
- [ ] 2. Legacy SOAP service (CoreWCF) with seeded data and anomalies
- [ ] 3. Core: model, validator, comparer, `SyncService` + unit tests
- [ ] 4. Infrastructure: EF Core + migrations, SOAP client
- [ ] 5. API endpoints + Swagger + health check
- [ ] 6. API tests (`WebApplicationFactory`)
- [ ] 7. README polish: diagram, screenshots, sample run output

## Possible next steps

- Incremental sync (`modifiedSince`)
- Scheduled background worker
- Retry policy (Polly) on the SOAP client
- Structured logging and correlation id per run
